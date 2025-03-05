import { Component, inject, input, OnInit, output } from '@angular/core';
import { Member } from '../../_models/member';
import { NgForOf, NgIf, NgClass, NgStyle, DecimalPipe } from '@angular/common';
import { FileUploader, FileUploadModule } from 'ng2-file-upload';
import { AccountService } from '../../_services/account.service';
import { environment } from '../../../environments/environment';
import { Photo } from '../../_models/photo';
import { MembersService } from '../../_services/members.service';

@Component({
  selector: 'app-photo-editor',
  imports: [NgIf, NgForOf, NgStyle, NgClass, FileUploadModule, DecimalPipe],
  templateUrl: './photo-editor.component.html',
  styleUrl: './photo-editor.component.css'
})
export class PhotoEditorComponent implements OnInit {
  private accountService = inject(AccountService);
  private memberService = inject(MembersService);
  member = input.required<Member>();
  uploader?: FileUploader;
  hasBaseDropzoneOver = false;
  baseUrl = environment.apiUrl;
  /* We need to update the member from the parent object, we can't update 'member' as it's an input, passed down from parent 
  memberChange emits an event when member has changed.*/
  memberChange = output<Member>();


  ngOnInit(): void {
    this.initializeUploader();
  }
  fileOverBase(e: any) {
    this.hasBaseDropzoneOver = e;
  }

  deletePhoto(photo: Photo) {
    this.memberService.deletePhoto(photo).subscribe({
      next: _ => {
        const updatedMember = { ...this.member() };
        /* .filter(p => p.id !== photoId) creates a new array that excludes the photo with the matching photoId. */
        updatedMember.photos = updatedMember.photos.filter(p => p.id !== photo.id);
        this.memberChange.emit(updatedMember);
      }
    });
  }

  setMainPhoto(photo: Photo) {
    this.memberService.setMainPhoto(photo).subscribe({
      next: (_: any) => {
        const user = this.accountService.currentUser();
        if (user) {
          user.photoUrl = photo.url;
          this.accountService.setCurrentUser(user);
        }
        const updatedMember = { ...this.member() };
        updatedMember.photoUrl = photo.url;
        updatedMember.photos.forEach((p) => {
          if (p.isMain) p.isMain = false;
          if (p.id === photo.id) p.isMain = true;
        });
        this.memberChange.emit(updatedMember);
      }
    })
  }

/* Basically the constructor */
  initializeUploader() {
    this.uploader = new FileUploader({
      url: this.baseUrl + 'users/add-photo',
      authToken: 'Bearer ' + this.accountService.currentUser()?.token,
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024,
    });

    this.uploader.onAfterAddingFile = (file) => {
      file.withCredentials = false;
    };

    this.uploader.onSuccessItem = (item, response, status, headers) => {
      const photo = JSON.parse(response);
      const updatedMember = { ...this.member() };
      updatedMember.photos.push(photo);
      this.memberChange.emit(updatedMember);
      if (photo.isMain) {
        const user = this.accountService.currentUser();
        if (user) {
          user.photoUrl = photo.url;
          this.accountService.setCurrentUser(user);
      }
      updatedMember.photoUrl = photo.url;
        updatedMember.photos.forEach((p) => {
          if (p.isMain) p.isMain = false;
          if (p.id === photo.id) p.isMain = true;
        });
        this.memberChange.emit(updatedMember);
    }
  }
}}
