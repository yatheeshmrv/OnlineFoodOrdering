import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-home',

  // Makes the routerLink directive available in this component's HTML.
  imports: [RouterLink],

  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class Home {
}