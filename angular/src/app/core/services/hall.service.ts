import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Hall } from '../models/hall.model';

@Injectable({
    providedIn: 'root'
})
export class HallService {

    private http = inject(HttpClient);

    private apiUrl =
        'https://localhost:44324/api/app/hall';

    createHall(data: {
        name: string;
        description: string;
        capacity: number;
        location: string;
        pricePerHour: number;
        status: number;
        type: number;
    }) {

        return this.http.post(
            this.apiUrl,
            data
        );

    }

    updateHall(
        id: string,
        hall: any
    ) {

        return this.http.put(
            `${this.apiUrl}/${id}`,
            hall
        );

    }

    getHalls(): Observable<any> {

        return this.http.get<any>(this.apiUrl);

    }

    deleteHall(id: string): Observable<void> {

        return this.http.delete<void>(
            `${this.apiUrl}/${id}`
        );

    }
}