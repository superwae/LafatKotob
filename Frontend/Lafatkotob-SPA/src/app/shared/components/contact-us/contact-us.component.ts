import { CommonModule } from '@angular/common';
import { Component, Directive } from '@angular/core';
import { FormsModule, NG_VALIDATORS, Validator, AbstractControl, ValidationErrors, FormGroup, FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AppUsereService } from '../../../Auth/services/appUserService/app-user.service';
import { SendEmailModel } from '../../Models/SendEmailModel';

// Directive to validate that the name does not contain numbers
@Directive({
  selector: '[appNoNumbers]',
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: NoNumbersDirective,
      multi: true
    }
  ]
})
export class NoNumbersDirective implements Validator {
  validate(control: AbstractControl): ValidationErrors | null {
    const hasNumber = /\d/;
    const value = control.value;

    if (hasNumber.test(value)) {
      return { 'hasNumber': true };
    }
    return null;
  }
}

// Component definition
@Component({
  selector: 'app-contact-us',
  standalone: true,
  imports: [RouterLink, CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './contact-us.component.html',
  styleUrls: ['./contact-us.component.css']
})
export class ContactUsComponent {
  email!: SendEmailModel;
  emailForm: FormGroup;
  loading = false;

  constructor(private appUserService: AppUsereService, private fb: FormBuilder) {
    this.emailForm = this.fb.group({
      name: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.pattern(/^[^\s@]+@[^\s@]+\.[^\s@]+$/)]],
      message: ['', [Validators.required]]
    });
  }

  onSubmit() {
    this.emailForm.markAllAsTouched();

    if (this.emailForm.valid) {
      this.loading = true;

      this.sendEmail(this.emailForm.value).then(() => {
        this.loading = false;
        this.emailForm.reset();
      }).catch(() => {
        this.loading = false;
        console.error('Failed to send email');
      });
    }
  }

  async sendEmail(formData: any) {
    this.email = formData;
    
      this.appUserService.sendEmail(this.email).subscribe({
          next:(response)=>{
            console.log("banana");
            
          }
      });
      console.log('Sending email to lftkotob@outlook.com:', formData);
    
      
    
  }
}
