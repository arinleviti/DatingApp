import { Component, inject, input, OnInit, output } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidatorFn, Validators } from '@angular/forms';
import { AccountService } from '../_services/account.service';
import { CommonModule, JsonPipe, NgIf } from '@angular/common';
import { TextInputComponent } from "../_forms/text-input/text-input.component";
import { DatePickerComponent } from "../_forms/date-picker/date-picker.component";
import { Router } from '@angular/router';
import { BrowserModule } from '@angular/platform-browser';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, TextInputComponent, NgIf, DatePickerComponent],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent implements OnInit {

  private accountService = inject(AccountService);
 cancelRegister = output<boolean>();
  model: any = {};
  private fb = inject(FormBuilder);
 /*  private toastr = inject(ToastrService); */
  private router = inject(Router);
  registerForm: FormGroup = new FormGroup({});
  maxDate = new Date();
  validationErrors: string[] = [];


  ngOnInit(): void {
    this.initializeForm();
    this.maxDate.setFullYear(this.maxDate.getFullYear() - 18);
  }

  initializeForm(){
    this.registerForm = this.fb.group({
      gender: ['male'],
      username: ['', Validators.required],
      knownAs: ['', Validators.required],
      dateOfBirth: ['', Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]],
      confirmPassword: ['', [Validators.required, this.matchValues('password')]]
    })
    /* this prevents the form to stay valid if password is changed a second time */
    this.registerForm.controls['password'].valueChanges.subscribe(() => {
      this.registerForm.controls['confirmPassword'].updateValueAndValidity();
    })
  }

  matchValues(matchTo: string): ValidatorFn {
    return (control: AbstractControl) => {  
      return control.value === control?.parent?.get(matchTo)?.value ? null : {isMatching: true} /* same isMatching in the html */
    }
  }

 register() {
  const dob = this.getDateOnly(this.registerForm.get('dateOfBirth')?.value);
  this.registerForm.patchValue({dateOfBirth: dob});
  this.accountService.register(this.registerForm.value).subscribe({
    next: _ => this.router.navigateByUrl('/members'),
    error: error => this.validationErrors = error
  })
 }

 cancel() {
  this.cancelRegister.emit(false);
 }

 private getDateOnly(dob: string | undefined) {
  if (!dob) return;
  return new Date(dob).toISOString().slice(0, 10);
 }
}

