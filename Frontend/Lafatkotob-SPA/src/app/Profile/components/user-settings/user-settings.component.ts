import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { AppUsereService } from '../../../Auth/services/appUserService/app-user.service';
import { AppUserModel } from '../../../Auth/Models/AppUserModel';
import { CommonModule } from '@angular/common';
import { ChangePasswordModel } from '../../../Auth/Models/ChangePasswordModel';
import { UpdateUserSettingModel } from '../../../Auth/Models/UpdateUserSettingModel';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { MatDialog } from '@angular/material/dialog';

@Component({
  selector: 'app-user-settings',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './user-settings.component.html',
  styleUrls: ['./user-settings.component.css'],
})
export class UserSettingsComponent implements OnInit {
  ChangePasswordModel!: ChangePasswordModel;
  UpdateUserSettingModel!: UpdateUserSettingModel;
  userForm: FormGroup;
  PasswordForm: FormGroup;
  isLoading: boolean = false;
  appuser!: AppUserModel;
  showPassword: boolean = false;
  router: any;
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
  isResendDisabled: boolean = false;
  resendTimer: number = 60;
  resendInterval: any;

  constructor(
    private fb: FormBuilder,
    private dialog: MatDialog,
    private appUserService: AppUsereService,
  ) {
    this.userForm = this.fb.group({
      name: [''],
      email: [
        '',
        [Validators.required, Validators.pattern(/^[^\s@]+@[^\s@]+\.[^\s@]+$/)],
      ],
      userName: ['', Validators.required],
      about: [''],
      city: ['', Validators.required],
    });
    this.PasswordForm = this.fb.group(
      {
        OldPassword: ['', Validators.required],
        NewPassword: ['', Validators.required],
        ConfirmPassword: ['', Validators.required],
      },
      { validators: this.checkPasswords },
    );
  }

  ngOnInit() {
    this.appUserService
      .getUserById(localStorage.getItem('userId')!)
      .subscribe((data) => {
        this.appuser = data;
        this.userForm.patchValue({
          name: this.appuser.name,
          email: this.appuser.email,
          userName: this.appuser.name,
          about: this.appuser.about,
          city: this.appuser.city,
        });
      });
  }

  updateUser() {
    this.userForm.markAllAsTouched();
    if (!this.userForm.valid) {
      return;
    }
    if (this.userForm.valid) {
      if (
        this.appuser.name === this.userForm.value.userName &&
        this.appuser.email === this.userForm.value.email &&
        this.appuser.about === this.userForm.value.about &&
        this.appuser.city === this.userForm.value.city
      ) {
        const dialogRef = this.dialog.open(ConfirmDialogComponent, {
          width: '1000px',
          data: {
            message: 'You have not made any changes.',
          },
        });

        dialogRef.afterClosed().subscribe((result: boolean) => {
          if (result) {
            return;
          } else {
            return;
          }
        });
        return;
      }
      // Create an instance of UpdateUserSettingModel using the form values
      const updateUserSettingData: UpdateUserSettingModel = {
        userId: localStorage.getItem('userId')!,
        name: this.userForm.value.userName,
        userName: this.userForm.value.userName,
        email: this.userForm.value.email,
        about: this.userForm.value.about,
        city: this.userForm.value.city,
      };

      // Assuming you have a method in your service to update the user settings
      this.appUserService.updateUserSettings(updateUserSettingData).subscribe({
        next: (response) => {
          const dialogRef = this.dialog.open(ConfirmDialogComponent, {
            width: '1000px',
            data: {
              message:
                'Your settings have been updated successfully. Please refresh the page to see the changes.',
            },
          });

          dialogRef.afterClosed().subscribe((result: boolean) => {
            if (result) {
              return;
            } else {
              return;
            }
          });
          return;
        },
        error: (error) => {
          console.error('Failed to update user settings', error);
          // Handle errors here, such as displaying an error message
        },
      });
    } else {
    }
  }

  changePassword() {
    if (this.PasswordForm.valid) {
      const changePasswordData: ChangePasswordModel = {
        oldPassword: this.PasswordForm.value.OldPassword,
        newPassword: this.PasswordForm.value.NewPassword,
        confirmPassword: this.PasswordForm.value.ConfirmPassword,
        userId: localStorage.getItem('userId')!,
      };

      this.appUserService.updatePassword(changePasswordData).subscribe({
        next: (response) => {},
        error: (error) => {
          console.error('Failed to update password', error);
        },
      });
    } else {
    }
  }

  resendEmail() {
    const userEmail = this.userForm.get('email')?.value;
    const userId = localStorage.getItem('userId');
    this.isResendDisabled = true;

    if (userEmail) {
      this.appUserService
        .resendVerificationEmail(userId!, userEmail)
        .subscribe({
          next: (response) => {
            // Start countdown timer
            this.resendInterval = setInterval(() => {
              this.resendTimer--;
              if (this.resendTimer === 0) {
                clearInterval(this.resendInterval);
                this.isResendDisabled = false;
                this.resendTimer = 60;
              }
            }, 1000);
          },
          error: (error) => {
            console.error('Failed to resend email', error);
          },
        });
    } else {
    }
  }

  ToggleShowPassword() {
    this.showPassword = !this.showPassword;
  }

  checkPasswords(group: FormGroup) {
    let pass = group.get('Password')?.value;
    let confirmPass = group.get('ConfirmNewPassword')?.value;
    return pass === confirmPass ? null : { notSame: true };
  }
}
