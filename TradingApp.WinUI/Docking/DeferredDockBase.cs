using Fluent;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace TradingApp.WinUI.Docking
{
    /// <summary>
    /// Base dock hỗ trợ:
    /// - Theo dõi trạng thái DockPanel (DockState, Visible, AutoHide, IsActivated...)
    /// - Deferred SetItems: nếu dock đang ẩn thì cache items, chỉ SetItems khi dock hiện lại
    /// - Logging (Serilog) có thể bật/tắt bằng IsLoggingAllowed
    ///
    /// Các dock con chỉ cần:
    /// - Gọi EnsureDockEventsHooked() trong OnLoad
    /// - Override OnItemsApplied(...) nếu cần ApplyRowStyles sau SetItems
    /// - Tự cấu hình view và hook inner list như bình thường.
    /// </summary>
    public abstract class DeferredDockBase<T> : FluentDockBase<T>
    {
        // ----- Logging control -----

        /// <summary>
        /// Bật/tắt log cho dock này.
        /// - Mặc định = false để tránh spam log.
        /// - Khi cần debug DockState / SetItems, set true từ bên ngoài (MainForm/DI).
        /// </summary>
        public bool IsLoggingAllowed = false;

        /// <summary>
        /// Helper ghi log:
        /// - Chỉ forward sang Serilog khi IsLoggingAllowed = true.
        /// - Dùng pattern messageTemplate + args giống Log.Information.
        /// </summary>
        protected void LogInfo(string messageTemplate, params object?[] args)
        {
            if (!IsLoggingAllowed)
                return;

            Log.Information(messageTemplate, args);
        }

        // ----- Trạng thái dock + deferred items -----

        private bool _dockEventsHooked;
        private bool _isFormClosed;

        // Flag & cache cho deferred SetItems
        private bool _pendingRefresh;
        private List<T>? _pendingItems;

        // Các DockState auto-hide (DockLeftAutoHide, DockRightAutoHide, DockTopAutoHide, DockBottomAutoHide)
        private static readonly DockState[] _autoHideStates =
        {
            DockState.DockBottomAutoHide,
            DockState.DockLeftAutoHide,
            DockState.DockRightAutoHide,
            DockState.DockTopAutoHide
        };

        /// <summary>
        /// Có đang ở mode AutoHide (DockLeftAutoHide, DockBottomAutoHide, ...) hay không.
        /// </summary>
        protected bool IsAutoHideMode()
            => _autoHideStates.Contains(DockState);

        /// <summary>
        /// Trạng thái "thực sự ẩn với user".
        /// Dùng để quyết định có nên vẽ lại List hay defer SetItems:
        /// - DockState.Hidden: đã Hide hoàn toàn
        /// - AutoHide nhưng panel chưa bật ra (IsActivated = false)
        /// - Control không Visible (form chưa show hoặc bị minimize)
        /// - Form đã bị dispose/closed
        /// </summary>
        protected bool IsHiddenForUser()
        {
            if (IsDisposed || _isFormClosed)
                return true;

            if (DockState == DockState.Hidden)
                return true;

            if (!Visible)
                return true;

            if (IsAutoHideMode() && !IsActivated)
                return true;

            return false;
        }

        /// <summary>
        /// Trạng thái tổng hợp public để bên ngoài (MainForm, service...)
        /// biết dock này có đang “thực sự hiển thị với user” hay không.
        /// TRUE  => có thể gọi SetItems nặng mà user nhìn thấy.
        /// FALSE => dock đang ẩn / AutoHide chưa bật / form chưa Visible / đã đóng.
        /// </summary>
        public bool IsVisibleForUser => !IsHiddenForUser();

        // ----- Hook event trạng thái DockPanel -----

        /// <summary>
        /// Gọi trong OnLoad của dock con để hook các event DockPanel:
        /// - DockStateChanged
        /// - VisibleChanged
        /// - FormClosed
        /// Hàm này đảm bảo chỉ hook 1 lần bằng flag _dockEventsHooked.
        /// </summary>
        protected void EnsureDockEventsHooked()
        {
            if (_dockEventsHooked)
                return;

            DockHandler.DockStateChanged += DockHandler_DockStateChanged;
            VisibleChanged += DeferredDockBase_VisibleChanged;
            FormClosed += DeferredDockBase_FormClosed;

            _dockEventsHooked = true;

            LogInfo(
                "[{Dock}] Dock events hooked. DockState={DockState}, Visible={Visible}, IsActivated={IsActivated}, IsHidden={IsHidden}",
                GetType().Name,
                DockState,
                Visible,
                IsActivated,
                IsHidden);
        }

        private void DockHandler_DockStateChanged(object? sender, EventArgs e)
        {
            LogInfo(
                "[{Dock}] DockStateChanged -> DockState={DockState}, Visible={Visible}, IsActivated={IsActivated}, IsHidden={IsHidden}, AutoHide={IsAutoHide}",
                GetType().Name,
                DockState,
                Visible,
                IsActivated,
                IsHidden,
                IsAutoHideMode());

            TryApplyPendingItems("DockStateChanged");
        }

        private void DeferredDockBase_VisibleChanged(object? sender, EventArgs e)
        {
            LogInfo(
                "[{Dock}] VisibleChanged -> Visible={Visible}, DockState={DockState}, IsActivated={IsActivated}, IsHidden={IsHidden}, AutoHide={IsAutoHide}",
                GetType().Name,
                Visible,
                DockState,
                IsActivated,
                IsHidden,
                IsAutoHideMode());

            if (Visible)
                TryApplyPendingItems("VisibleChanged");
        }

        private void DeferredDockBase_FormClosed(object? sender, FormClosedEventArgs e)
        {
            _isFormClosed = true;

            LogInfo(
                "[{Dock}] FormClosed. DockState={DockState}, Visible={Visible}, IsActivated={IsActivated}, IsHidden={IsHidden}, AutoHide={IsAutoHide}",
                GetType().Name,
                DockState,
                Visible,
                IsActivated,
                IsHidden,
                IsAutoHideMode());
        }

        /// <summary>
        /// Intercept OnActivated ở base:
        /// - Log trạng thái
        /// - Thử apply pending items (case AutoHide bật panel lên)
        /// - Sau đó gọi OnDockActivated(...) cho dock con override nếu cần thêm behavior.
        /// </summary>
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            LogInfo(
                "[{Dock}] OnActivated. DockState={DockState}, Visible={Visible}, IsActivated={IsActivated}, IsHidden={IsHidden}, AutoHide={IsAutoHide}",
                GetType().Name,
                DockState,
                Visible,
                IsActivated,
                IsHidden,
                IsAutoHideMode());

            TryApplyPendingItems("OnActivated");
            OnDockActivated(e);
        }

        /// <summary>
        /// Intercept OnDeactivate ở base:
        /// - Log trạng thái
        /// - Gọi OnDockDeactivated(...) cho dock con override nếu cần.
        /// </summary>
        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);

            LogInfo(
                "[{Dock}] OnDeactivate. DockState={DockState}, Visible={Visible}, IsActivated={IsActivated}, IsHidden={IsHidden}, AutoHide={IsAutoHide}",
                GetType().Name,
                DockState,
                Visible,
                IsActivated,
                IsHidden,
                IsAutoHideMode());

            OnDockDeactivated(e);
        }

        /// <summary>
        /// Hook cho dock con nếu muốn xử lý thêm khi dock được Activate.
        /// </summary>
        protected virtual void OnDockActivated(EventArgs e) { }

        /// <summary>
        /// Hook cho dock con nếu muốn xử lý thêm khi dock Deactivate.
        /// </summary>
        protected virtual void OnDockDeactivated(EventArgs e) { }

        // ----- Deferred SetItems -----

        /// <summary>
        /// Thử áp dụng lại SetItems nếu đang có pending data
        /// và dock đã "hiện" ra với user (không còn hidden).
        /// </summary>
        protected void TryApplyPendingItems(string reason)
        {
            if (!_pendingRefresh || _pendingItems == null || _pendingItems.Count == 0)
            {
                LogInfo(
                    "[{Dock}] TryApplyPendingItems skipped ({Reason}). Nothing pending. DockState={DockState}, Visible={Visible}, IsActivated={IsActivated}, IsHidden={IsHidden}, AutoHide={IsAutoHide}",
                    GetType().Name,
                    reason,
                    DockState,
                    Visible,
                    IsActivated,
                    IsHidden,
                    IsAutoHideMode());
                return;
            }

            if (IsHiddenForUser())
            {
                LogInfo(
                    "[{Dock}] TryApplyPendingItems skipped ({Reason}). Still hidden. DockState={DockState}, Visible={Visible}, IsActivated={IsActivated}, IsHidden={IsHidden}, AutoHide={IsAutoHide}",
                    GetType().Name,
                    reason,
                    DockState,
                    Visible,
                    IsActivated,
                    IsHidden,
                    IsAutoHideMode());
                return;
            }

            var items = _pendingItems;
            _pendingItems = null;
            _pendingRefresh = false;

            LogInfo(
                "[{Dock}] TryApplyPendingItems APPLIED ({Reason}). Count={Count}",
                GetType().Name,
                reason,
                items.Count);

            base.SetItems(items);
            OnItemsApplied(items);
        }

        /// <summary>
        /// Hook cho dock con override nếu cần ApplyRowStyles, v.v. sau mỗi lần SetItems thực tế.
        /// </summary>
        protected virtual void OnItemsApplied(IReadOnlyList<T> items)
        {
            // mặc định không làm gì, dock con override nếu cần
        }

        /// <summary>
        /// Override SetItems:
        /// - Nếu dock đang hidden: cache vào _pendingItems và chỉ log "DEFERRED".
        /// - Khi dock Un-hide / Activate / Visible: TryApplyPendingItems sẽ SetItems thực.
        /// - Nếu dock đang hiển thị: SetItems ngay + gọi OnItemsApplied.
        /// </summary>
        public override void SetItems(IEnumerable<T> items)
        {
            var list = items?.ToList() ?? new List<T>();

            if (IsHiddenForUser())
            {
                _pendingItems = list;
                _pendingRefresh = true;

                LogInfo(
                    "[{Dock}] SetItems DEFERRED (dock hidden). DockState={DockState}, Visible={Visible}, IsActivated={IsActivated}, IsHidden={IsHidden}, AutoHide={IsAutoHide}, Count={Count}",
                    GetType().Name,
                    DockState,
                    Visible,
                    IsActivated,
                    IsHidden,
                    IsAutoHideMode(),
                    list.Count);

                return;
            }

            LogInfo(
                "[{Dock}] SetItems APPLIED immediately. DockState={DockState}, Visible={Visible}, IsActivated={IsActivated}, IsHidden={IsHidden}, AutoHide={IsAutoHide}, Count={Count}",
                GetType().Name,
                DockState,
                Visible,
                IsActivated,
                IsHidden,
                IsAutoHideMode(),
                list.Count);

            base.SetItems(list);
            OnItemsApplied(list);
        }
    }
}
