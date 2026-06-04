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

    getHalls(): Observable<any> {

        return this.http.get<any>(this.apiUrl);

    }

}