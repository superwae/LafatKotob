import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AppUsereService } from '../../services/appUserService/app-user.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [FormsModule, ReactiveFormsModule, RouterModule, CommonModule],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.css',
})
export class ResetPasswordComponent implements OnInit {
  resetPasswordForm: FormGroup;
  token: string = '';
  email: string = '';

  constructor(
    private fb: FormBuilder,
    private AppUserService: AppUsereService,
    private route: ActivatedRoute,
    private router: Router,
  ) {
    this.resetPasswordForm = this.fb.group(
      {
        newPassword: [
          '',
          [
            Validators.required,
            Validators.pattern('^(?=.*[A-Za-z])(?=.*\\d).{8,}$'),
          ],
        ],
        confirmPassword: ['', [Validators.required]],
      },
      { validator: this.passwordMatchValidator },
    );
  }

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      this.token = params['token'];
      this.email = params['email'];

      if (!this.token || !this.email) {
        // Handle the absence of token or email, perhaps redirecting the user or showing an error
        console.error('Token or email not provided in URL');
        // this.router.navigate(['/']); // Redirect to home or error page
      }
    });
  }

  passwordMatchValidator(fg: FormGroup): { [key: string]: boolean } | null {
    const newPassword = fg.get('newPassword')?.value;
    const confirmPassword = fg.get('confirmPassword')?.value;
    if (newPassword && confirmPassword && newPassword !== confirmPassword) {
      return { passwordMismatch: true };
    }
    return null;
  }

  public userMessage: string = '';
  public isSuccess: boolean = false;

  submitResetPassword(): void {
    if (this.resetPasswordForm.valid && this.email && this.token) {
      const resetData = {
        email: this.email,
        token: this.token,
        newPassword: this.resetPasswordForm.value.newPassword,
        confirmPassword: this.resetPasswordForm.value.confirmPassword,
      };

      this.AppUserService.resetPassword(resetData).subscribe({
        next: (response) => {
          this.userMessage = 'Your password has been successfully reset.';
          this.isSuccess = true;
          // Optionally wait a few seconds, then navigate to the login page
          setTimeout(() => this.router.navigate(['/login']), 5000);
        },
        error: (error) => {
          if (error.status === 429) {
            // Specific message for rate limit errors
            this.userMessage =
              'You have made too many requests. Please wait a while before trying again.';
          } else {
            // Generic error message for other errors
            this.userMessage =
              'The password reset link is invalid or has expired. Please request a new link.';
          }
          this.isSuccess = false;
        },
      });
    }
  }
}
