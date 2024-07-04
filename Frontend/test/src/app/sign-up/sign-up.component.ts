import { Component } from '@angular/core';
import { FormGroup, FormControl, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http'; 
import { AppUserServiceService } from '../Services/AppUserService/app-user-service.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-sign-up',
  standalone: true,
  imports: [ReactiveFormsModule,FormsModule,HttpClientModule],
  templateUrl: './sign-up.component.html',
  styleUrls: ['./sign-up.component.css'] // Correct the property name here
})
export class SignUpComponent {
  signUpForm: FormGroup;
  
  constructor(private appuserSerivece: AppUserServiceService,private router: Router) {
    
    this.signUpForm = new FormGroup({
      role: new FormControl('', Validators.required),
      name: new FormControl('', Validators.required),
      password: new FormControl('', [Validators.required, Validators.minLength(8)]),
      confirmNewPassword: new FormControl('', Validators.required),
      email: new FormControl('', [Validators.required, Validators.email]),
      confirmNewEmail: new FormControl('', Validators.required),
      userName: new FormControl('', Validators.required),
      profilePictureUrl: new FormControl(''),
      dthDate: new FormControl('', Validators.required),
      city: new FormControl(''),
      about: new FormControl('')
    });
  }
  onSubmit(): void {
    if (this.signUpForm.valid) {
      // Access role from the form's value
      const role = this.signUpForm.value.role;
  
      this.appuserSerivece.createUser(this.signUpForm.value, role).subscribe({
        next: (user) => {
          console.log(user);
          // Handle successful registration, e.g., navigate to a different page
          // this.router.navigate(['/success-page']);
        },
        error: (error) => {
          console.error(error);
          // Handle error, e.g., show a message to the user
        }
      });
    } else {
      // Handle the case where the form is invalid
      console.error('Form is invalid');
    }
  }
  
  
}