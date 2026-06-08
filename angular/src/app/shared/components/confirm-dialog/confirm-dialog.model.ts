export type ConfirmDialogType = 'confirm' | 'complete' | 'cancel' | 'delete';

export interface ConfirmDialogData {
  title: string;
  message: string;
  type?: ConfirmDialogType;
  icon?: string;
  confirmLabel?: string;
  cancelLabel?: string;
  warningMessage?: string;
}

interface ConfirmDialogPreset {
  type: ConfirmDialogType;
  icon: string;
  confirmLabel: string;
  cancelLabel: string;
}

export const CONFIRM_DIALOG_PRESETS: Record<
  ConfirmDialogType,
  ConfirmDialogPreset
> = {
  confirm: {
    type: 'confirm',
    icon: 'check_circle',
    confirmLabel: 'تأكيد الحجز',
    cancelLabel: 'إلغاء',
  },
  complete: {
    type: 'complete',
    icon: 'task_alt',
    confirmLabel: 'إكمال الحجز',
    cancelLabel: 'إلغاء',
  },
  cancel: {
    type: 'cancel',
    icon: 'warning_amber',
    confirmLabel: 'إلغاء الحجز',
    cancelLabel: 'رجوع',
  },
  delete: {
    type: 'delete',
    icon: 'delete',
    confirmLabel: 'حذف',
    cancelLabel: 'إلغاء',
  },
};

export function resolveConfirmDialogData(
  data: ConfirmDialogData
): Required<
  Pick<ConfirmDialogData, 'type' | 'icon' | 'confirmLabel' | 'cancelLabel'>
> &
  ConfirmDialogData {
  const type = data.type ?? 'delete';
  const preset = CONFIRM_DIALOG_PRESETS[type];

  return {
    ...data,
    type,
    icon: data.icon ?? preset.icon,
    confirmLabel: data.confirmLabel ?? preset.confirmLabel,
    cancelLabel: data.cancelLabel ?? preset.cancelLabel,
  };
}
