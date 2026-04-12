import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HomeSummary } from '../models/home-summary.model';

@Injectable({ providedIn: 'root' })
export class HomeService {
    private readonly http = inject(HttpClient);

    getSummary(month: number, year: number): Observable<HomeSummary> {
        return this.http.get<HomeSummary>(`/api/home/summary?month=${month}&year=${year}`);
    }
}
