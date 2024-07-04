import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Router, RouterLink, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AppUsereService } from '../../services/appUserService/app-user.service';
import { LoginResponse } from '../../Models/Loginresponse';
import { GenreService } from '../../services/GenreService/genre.service';
import { registerModel } from '../../Models/registerModel';
import { animate, style, transition, trigger } from '@angular/animations';
import {
  FontAwesomeModule,
  FaIconLibrary,
} from '@fortawesome/angular-fontawesome';
import { fas } from '@fortawesome/free-solid-svg-icons';
@Component({
  selector: 'app-login-register',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    CommonModule,
    FormsModule,
    RouterModule,
    RouterLink,
    FontAwesomeModule,
  ],
  templateUrl: './login-register.component.html',
  styleUrl: './login-register.component.css',
  animations: [
    trigger('fade', [
      transition('void => active', [
        // using status here for transition
        style({ opacity: 0 }),
        animate(500, style({ opacity: 1 })),
      ]),
      transition('* => void', [animate(500, style({ opacity: 0 }))]),
    ]),
  ],
})
export class LoginRegisterComponent implements OnInit {
  loginForm: FormGroup;
  registerForm: FormGroup;
  isLoading: boolean = false;
  loginErrorMessage: string | null = null;
  userDetails: registerModel | null = null;
  selectedFile: File | null = null;
  currentfileds: 'login' | 'signup' = 'login';
  cities: string[] = [
    'Ramallah',
    'Jerusalem',
    'Gaza',
    'Nablus',
    'Hebron',
    'Jenin',
    'Tulkarm',
    'Qalqilya',
    'Bethlehem',
    'Jericho',
    'Tubas',
    'Salfit',
  ]; // List of cities
  selectedCity: string = '';

  constructor(
    private fb: FormBuilder,
    private AppUserService: AppUsereService,
    private router: Router,
    private userPreferences: GenreService,
    library: FaIconLibrary,
  ) {
    library.addIconPacks(fas);
    this.router.events.subscribe((event) => {});
    this.loginForm = this.fb.group({
      UserName: ['', [Validators.required]],
      Password: ['', [Validators.required]],
    });

    this.registerForm = this.fb.group(
      {
        Name: [''],
        UserName: ['', [Validators.required]],
        Email: [
          '',
          [
            Validators.required,
            Validators.pattern(/^[^\s@]+@[^\s@]+\.[^\s@]+$/),
          ],
        ],
        Password: [
          '',
          [
            Validators.required,
            Validators.pattern('^(?=.*[A-Za-z])(?=.*\\d).{8,}$'),
          ],
        ],
        ConfirmNewPassword: [''],
        ConfirmNewEmail: [''],
        DTHDate: ['', [Validators.required]],
        City: ['', [Validators.required]],
        About: ['test'],
      },
      { validators: this.checkPasswords },
    );
  }

  ngOnInit(): void {}

  login(): void {
    this.loginForm.markAllAsTouched();
    this.isLoading = true;
    this.loginErrorMessage = null;
    if (this.loginForm.valid) {
      this.AppUserService.loginUser(this.loginForm.value).subscribe({
        next: (response: LoginResponse) => {
          this.isLoading = false;
        },
        error: (error) => {
          console.error(error);
          this.loginErrorMessage = 'Invalid username or password.';
          this.isLoading = false;
        },
      });
    } else {
      this.isLoading = false;
    }
  }

  toggleForm() {
    if (this.currentfileds === 'login') {
      this.currentfileds = 'signup';
    } else {
      this.currentfileds = 'login';
    }
  }

  Register(): void {
    Object.keys(this.registerForm.controls).forEach((field) => {
      const control = this.registerForm.get(field);
      control?.markAsTouched();
      control?.updateValueAndValidity();
    });

    if (this.registerForm.valid) {
      const userDetails: registerModel = {
        Name: this.registerForm.value.UserName,
        UserName: this.registerForm.value.UserName,
        Email: this.registerForm.value.Email,
        Password: this.registerForm.value.Password,
        ConfirmNewPassword: this.registerForm.value.ConfirmNewPassword,
        DthDate: this.registerForm.value.DTHDate,
        City: this.registerForm.value.City,
        About: this.registerForm.value.About,
        ConfirmNewEmail: this.registerForm.value.Email,
        ProfilePictureUrl: 'test',
      };

      this.userPreferences.setUserDetails(userDetails);

      this.router.navigate(['/userPreferences']);
    }
  }
  checkPasswords(group: FormGroup) {
    let pass = group.get('Password')?.value;
    let confirmPass = group.get('ConfirmNewPassword')?.value;
    return pass === confirmPass ? null : { notSame: true };
  }
}
