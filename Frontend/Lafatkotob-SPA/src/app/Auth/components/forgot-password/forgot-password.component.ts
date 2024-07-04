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
  selector: 'app-forgot-password',
  standalone: true,
  imports: [ReactiveFormsModule, RouterModule, CommonModule, FormsModule],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.css',
})
export class ForgotPasswordComponent {
  forgotPasswordForm: FormGroup;
  constructor(
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private AppUserService: AppUsereService,
  ) {
    this.forgotPasswordForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
    });
  }
  // In your ForgotPasswordComponent

  // Add a property to hold the message for the user
  public userMessage: string | null = null;
  public errorMessage: string | null = null;

  submitForgotPassword(): void {
    if (this.forgotPasswordForm.valid) {
      this.AppUserService.forgotPassword(
        this.forgotPasswordForm.value.email,
      ).subscribe(
        (response) => {
          // Notify user with a generic message
          this.userMessage =
            'If your email address is registered with us, you will receive an email.';
          this.errorMessage = '';
          this.forgotPasswordForm.reset();
        },
        (error) => {
          if (error.status === 429) {
            // Specific message for rate limit errors
            this.userMessage = '';
            this.errorMessage =
              'You have made too many requests. Please wait a while before trying again.';
          } else {
            // Generic error message for other errors
            this.userMessage =
              'An error occurred while attempting to perform password reset. Please try again later.';
            this.errorMessage = '';
          }
        },
      );
    } else {
      // Notify user to fill in the form correctly
      this.userMessage = 'Please fill in the form correctly.';
    }
  }
}
