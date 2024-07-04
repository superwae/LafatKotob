import { Component } from '@angular/core';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { AppUserServiceService } from '../Services/AppUserService/app-user-service.service';
import { Router } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule,FormsModule,HttpClientModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  signin: FormGroup;
  
  constructor(private appuserSerivece: AppUserServiceService,private router: Router) {
    
    this.signin = new FormGroup({
      userName: new FormControl('', Validators.required),
      password: new FormControl('', [Validators.required, Validators.minLength(8)]),
     

    });
  }

  onSubmit(): void {
    if (this.signin.valid) {
      // Access role from the form's value
      const role = this.signin.value.role;
  
      this.appuserSerivece.loginUser(this.signin.value).subscribe({
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
