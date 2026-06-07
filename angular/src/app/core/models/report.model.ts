export interface ReportSummary {
  totalReservations: number;
  totalRevenue: number;
  averageReservationValue: number;
  activeCustomers: number;
}

export interface RevenuePeriod {
  amount: number;
  changePercent: number | null;
}

export interface RevenueAnalytics {
  today: RevenuePeriod;
  thisMonth: RevenuePeriod;
  thisYear: RevenuePeriod;
}

export interface ReservationStatistics {
  pendingCount: number;
  confirmedCount: number;
  cancelledCount: number;
  completedCount: number;
}

export interface HallPerformance {
  hallName: string;
  reservationCount: number;
  revenue: number;
  averageGuests: number;
  occupancyRate: number;
}

export interface CustomerActivity {
  customerName: string;
  reservationCount: number;
  totalSpent: number;
}

export interface MonthlyRevenue {
  year: number;
  month: number;
  monthLabel: string;
  reservationCount: number;
  revenue: number;
}

export interface ReportsResult {
  summary: ReportSummary;
  revenueAnalytics: RevenueAnalytics;
  reservationStatistics: ReservationStatistics;
  hallPerformance: HallPerformance[];
  customerActivity: CustomerActivity[];
  monthlyReport: MonthlyRevenue[];
}

export interface ReportFilters {
  dateFrom: string;
  dateTo: string;
  hallId: string | null;
  status: number | null;
}

export interface ReportStatusOption {
  value: number | null;
  label: string;
}

export const REPORT_STATUS_OPTIONS: ReportStatusOption[] = [
  { value: null, label: 'جميع الحالات' },
  { value: 1, label: 'قيد الانتظار' },
  { value: 2, label: 'مؤكد' },
  { value: 3, label: 'ملغي' },
  { value: 4, label: 'مكتمل' },
];
