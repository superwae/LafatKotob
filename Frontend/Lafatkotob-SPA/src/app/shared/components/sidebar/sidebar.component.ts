import { Component, HostListener, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ModaleService } from '../../Service/ModalService/modal.service';
import { Observable, Subscription } from 'rxjs';
import { CommonModule } from '@angular/common';
import { throttleTime } from 'rxjs/operators';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, CommonModule],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.css',
})
export class SidebarComponent implements OnInit, OnDestroy {
  show: boolean = true;
  username: string = '';
  username2: string = '';
  private subscription: Subscription = new Subscription();
  private scrollSubscription!: Subscription;

  constructor(
    private route: ActivatedRoute,
    private modalService: ModaleService,
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      this.username = params['username'];
      this.username2 = localStorage.getItem('userName')!;
    });

    this.subscription.add(
      this.modalService.showModal$.subscribe((visible) => {
        this.show = visible ? false : true;
      }),
    );

    this.scrollSubscription = new Subscription();
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
    this.scrollSubscription.unsubscribe();
  }
}
