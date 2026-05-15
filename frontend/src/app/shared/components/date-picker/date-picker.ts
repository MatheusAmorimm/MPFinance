import {
    ChangeDetectionStrategy,
    Component,
    computed,
    ElementRef,
    forwardRef,
    inject,
    input,
    signal,
} from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

interface CalendarDay {
    iso:          string;
    day:          number;
    currentMonth: boolean;
    isToday:      boolean;
    isSelected:   boolean;
}

function toIso(d: Date): string {
    const y  = d.getFullYear();
    const m  = String(d.getMonth() + 1).padStart(2, '0');
    const dd = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${dd}`;
}

const MONTHS_PT = [
    'Janeiro','Fevereiro','Março','Abril','Maio','Junho',
    'Julho','Agosto','Setembro','Outubro','Novembro','Dezembro',
];

@Component({
    selector: 'app-date-picker',
    changeDetection: ChangeDetectionStrategy.OnPush,
    templateUrl: './date-picker.html',
    styleUrl:    './date-picker.scss',
    providers: [{
        provide:     NG_VALUE_ACCESSOR,
        useExisting: forwardRef(() => DatePickerComponent),
        multi:       true,
    }],
    host: {
        '(document:click)': 'onOutsideClick($event)',
    },
})
export class DatePickerComponent implements ControlValueAccessor {
    private el = inject(ElementRef);

    /** Id forwarded to the trigger (links <label for="..."> ) */
    inputId  = input<string>('');
    hasError = input<boolean>(false);

    readonly open       = signal(false);
    readonly openUpward = signal(false);
    readonly value      = signal('');
    readonly viewYear   = signal(new Date().getFullYear());
    readonly viewMonth  = signal(new Date().getMonth());
    readonly disabled   = signal(false);

    readonly WEEKDAYS = ['Dom', 'Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb'];

    private onChange:   (v: string) => void = () => {};
    private onTouched: ()          => void = () => {};

    /* ─── Computed ───────────────────────────────── */

    readonly displayValue = computed(() => {
        const v = this.value();
        if (!v) return '';
        const [y, m, d] = v.split('-');
        return `${d}/${m}/${y}`;
    });

    readonly monthLabel = computed(() =>
        `${MONTHS_PT[this.viewMonth()]} ${this.viewYear()}`
    );

    readonly calendarDays = computed<CalendarDay[]>(() => {
        const year     = this.viewYear();
        const month    = this.viewMonth();
        const selected = this.value();
        const todayStr = toIso(new Date());

        const days: CalendarDay[] = [];
        const firstDay = new Date(year, month, 1);
        const lastDay  = new Date(year, month + 1, 0);
        const startDow = firstDay.getDay(); // 0=Sun

        const push = (d: Date, currentMonth: boolean) => {
            const iso = toIso(d);
            days.push({ iso, day: d.getDate(), currentMonth, isToday: iso === todayStr, isSelected: iso === selected });
        };

        for (let i = startDow; i > 0; i--)       push(new Date(year, month, 1 - i),    false);
        for (let d = 1; d <= lastDay.getDate(); d++) push(new Date(year, month, d),      true);
        const remaining = 42 - days.length;
        for (let d = 1; d <= remaining; d++)      push(new Date(year, month + 1, d),   false);

        return days;
    });

    /* ─── Actions ────────────────────────────────── */

    toggle(): void {
        if (this.disabled()) return;
        if (!this.open()) {
            const v = this.value();
            if (v) {
                const [y, m] = v.split('-').map(Number);
                this.viewYear.set(y);
                this.viewMonth.set(m - 1);
            } else {
                const now = new Date();
                this.viewYear.set(now.getFullYear());
                this.viewMonth.set(now.getMonth());
            }
            const rect = (this.el.nativeElement as HTMLElement).getBoundingClientRect();
            const spaceBelow = window.innerHeight - rect.bottom;
            this.openUpward.set(spaceBelow < 320);
        }
        this.open.update(o => !o);
        this.onTouched();
    }

    selectDay(day: CalendarDay, event: MouseEvent): void {
        event.stopPropagation();
        this.value.set(day.iso);
        this.onChange(day.iso);
        this.open.set(false);
    }

    prevMonth(event: MouseEvent): void {
        event.stopPropagation();
        if (this.viewMonth() === 0) { this.viewMonth.set(11); this.viewYear.update(y => y - 1); }
        else                          this.viewMonth.update(m => m - 1);
    }

    nextMonth(event: MouseEvent): void {
        event.stopPropagation();
        if (this.viewMonth() === 11) { this.viewMonth.set(0); this.viewYear.update(y => y + 1); }
        else                           this.viewMonth.update(m => m + 1);
    }

    onOutsideClick(event: MouseEvent): void {
        if (!this.el.nativeElement.contains(event.target as Node)) {
            this.open.set(false);
        }
    }

    /* ─── ControlValueAccessor ───────────────────── */

    writeValue(value: string | null): void   { this.value.set(value ?? ''); }
    registerOnChange(fn: (v: string) => void): void { this.onChange = fn; }
    registerOnTouched(fn: () => void): void  { this.onTouched = fn; }
    setDisabledState(isDisabled: boolean): void { this.disabled.set(isDisabled); }
}
