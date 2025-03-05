import { NgIf } from '@angular/common';
import { Component, input, Self } from '@angular/core';
import { ControlValueAccessor, FormControl, NgControl, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-text-input',
  standalone: true,
  imports: [NgIf, ReactiveFormsModule],
  templateUrl: './text-input.component.html',
  styleUrl: './text-input.component.css'
})
export class TextInputComponent implements ControlValueAccessor {

  label = input<string>('');
  type = input<string>('text');


  /* ngControl is injected automatically by Angular when this component is used inside a form. 
  Self makes sure that each instance of this component gets its own instance of NgControl.
   The NgControl class is what essentially allows a component to become part of the Angular form control system. Let’s break this down a bit more*/
  constructor(@Self() public ngControl: NgControl) {
    this.ngControl.valueAccessor = this
  }

  writeValue(obj: any): void {

  }
  registerOnChange(fn: any): void {

  }
  registerOnTouched(fn: any): void {

  }
setDisabledState?(isDisabled: boolean): void {
}

  /* We need to use this getter to avoid a bug in the html */
  get control(): FormControl {
    return this.ngControl.control as FormControl
  }

}
