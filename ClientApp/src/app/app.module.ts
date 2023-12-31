import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';

import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { AppComponent } from './app.component';
import { HomeComponent } from './home/home.component';
import { LoginComponent } from './login/login.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';

import { FormsModule } from '@angular/forms';
import { MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import { AuthGuardService } from './Services/auth-guard.service';
import { AdditionalFormRequestComponent } from './additional-form-request/additional-form-request.component';
import { CarOverviewComponent } from './car-overview/car-overview.component';
import { NotificationComponent } from './notification/notification.component';
import { RequestFormComponent } from './request-form/request-form.component';
import { RequestOverviewComponent } from './request-overview/request-overview.component';
import { VehicleDialogComponent } from './vehicle-dialog/vehicle-dialog.component';
import { FinanceOverviewComponent } from './finance-overview/finance-overview.component';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    LoginComponent,
    RequestFormComponent,
    RequestOverviewComponent,
    AdditionalFormRequestComponent,
    NotificationComponent,
    CarOverviewComponent,
    VehicleDialogComponent,
    FinanceOverviewComponent,
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    MatMenuModule,
    MatButtonModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    MatIconModule,
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'login', component: LoginComponent },
      {
        path: 'requests-overview/:departmentId',
        component: RequestOverviewComponent,
        canActivate: [AuthGuardService],
      },
      {
        path: 'vehicles-overview',
        component: CarOverviewComponent,
        canActivate: [AuthGuardService],
      },
      {
        path: 'accounting-overview',
        component: FinanceOverviewComponent,
        canActivate: [AuthGuardService],
      },
      { path: 'request-form/:id', component: RequestFormComponent },
    ]),
    BrowserAnimationsModule,
  ],
  providers: [],
  bootstrap: [AppComponent],
})
export class AppModule {}
