import { Component, inject, OnInit, ViewChild } from '@angular/core';
import { MembersService } from '../../_services/members.service';
import { ActivatedRoute } from '@angular/router';
import { Member } from '../../_models/member';
import { TabDirective, TabsetComponent, TabsModule } from 'ngx-bootstrap/tabs';
import { GalleryItem, GalleryModule, ImageItem} from 'ng-gallery';
import { TimeagoModule } from 'ngx-timeago';
import { DatePipe } from '@angular/common';
import { MemberMessagesComponent } from "../member-messages/member-messages.component";
import { Message } from '../../_models/message';
import { MessageService } from '../../_services/message.service';

@Component({
  selector: 'app-member-detail',
  imports: [TabsModule, GalleryModule, TimeagoModule, DatePipe, MemberMessagesComponent],
  templateUrl: './member-detail.component.html',
  styleUrl: './member-detail.component.css'
})
export class MemberDetailComponent implements OnInit {
  private messageService = inject(MessageService)
 @ViewChild('memberTabs', {static: true}) memberTabs?: TabsetComponent
 activeTab?: TabDirective;
 messages: Message[] = []
 private memberService = inject(MembersService);
 /* What Does ActivatedRoute Do?
It provides access to route-specific data, such as:

Query parameters (?tab=Messages)
Route parameters (/members/:id)
Route data (resolve data from the router) */
 private route = inject(ActivatedRoute)


 member: Member= {} as Member;
 images: GalleryItem[] = []

ngOnInit(): void {
   /* The resolver fetches the Member data before MemberDetailComponent is loaded, 
 ensuring that the component already has the member object available when it initializes.
 When using a resolver, the resolved data is attached to the data property of the ActivatedRoute and is wrapped inside an observable. 
 That's why you need to subscribe to this.route.data to access it.
 this.route.data.subscribe() gets the resolved member data from the resolver */
this.route.data.subscribe({
  next: data => {
    this.member = data['member'];
    this.member && this.member.photos.map(p => { this.images.push(new ImageItem({src: p.url, thumb: p.url}))})
  }
})
   /* this.route.queryParams is an Observable that emits whenever the query parameters in the URL change. 
   */
   this.route.queryParams.subscribe({
    next: params => {
      params['tab'] && this.selectTab(params['tab'])
    }
   })
 }

 onUpdateMessages (event: Message){
  this.messages.push(event);
 }

 selectTab(heading: string){
  if (this.memberTabs){
    const messageTab = this.memberTabs.tabs.find(x => x.heading === heading);
    if (messageTab) messageTab.active = true;
  }
 }

 onTabActivated(data: TabDirective){
  this.activeTab = data;
  if (this.activeTab.heading === 'Messages' && this.messages.length === 0 && this.member) {
    this.messageService.getMessageThread(this.member.username).subscribe({
      next: messages => this.messages = messages
    })
  }
 }

 /* loadMember() {
  const username = this.route.snapshot.paramMap.get('username');
  if (!username) return;
  this.memberService.getMember(username).subscribe({
    next: member => {
      this.member = member;
      member.photos.map(p => { this.images.push(new ImageItem({src: p.url, thumb: p.url}))})
    }
  })
 } */
 }

