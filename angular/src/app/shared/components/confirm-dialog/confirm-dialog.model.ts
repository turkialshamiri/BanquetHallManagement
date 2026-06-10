import { AppLocalizationService } from 'src/app/core/services/app-localization.service';

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
  confirmLabelKey: string;
  cancelLabelKey: string;
}

export const CONFIRM_DIALOG_PRESET_KEYS: Record<
  ConfirmDialogType,
  ConfirmDialogPreset
> = {
  confirm: {
    type: 'confirm',
    icon: 'check_circle',
    confirmLabelKey: 'ConfirmDialog:ConfirmReservation',
    cancelLabelKey: 'Cancel',
  },
  complete: {
    type: 'complete',
    icon: 'task_alt',
    confirmLabelKey: 'ConfirmDialog:CompleteReservation',
    cancelLabelKey: 'Cancel',
  },
  cancel: {
    type: 'cancel',
    icon: 'warning_amber',
    confirmLabelKey: 'ConfirmDialog:CancelReservation',
    cancelLabelKey: 'Dialog:Back',
  },
  delete: {
    type: 'delete',
    icon: 'delete',
    confirmLabelKey: 'Delete',
    cancelLabelKey: 'Cancel',
  },
};

export function resolveConfirmDialogData(
  data: ConfirmDialogData,
  l10n: AppLocalizationService
): Required<
  Pick<ConfirmDialogData, 'type' | 'icon' | 'confirmLabel' | 'cancelLabel'>
> &
  ConfirmDialogData {
  const type = data.type ?? 'delete';
  const preset = CONFIRM_DIALOG_PRESET_KEYS[type];

  return {
    ...data,
    type,
    icon: data.icon ?? preset.icon,
    confirmLabel:
      data.confirmLabel ?? l10n.instant(preset.confirmLabelKey),
    cancelLabel: data.cancelLabel ?? l10n.instant(preset.cancelLabelKey),
  };
}
