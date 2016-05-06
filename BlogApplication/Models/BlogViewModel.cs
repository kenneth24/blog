﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BlogApplication.Models
{
    /*  things to do / mga meron sa blogsite based
     *  Post, list of post
     *  category
     *  comment
     *  reply (reply to reply kung may time)
     *  image or video
     *  tags - para sa seo search
     *  like
     */

    public class Post
    {
        public string Id { get; set; }
        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; }
        [Required]
        [Display(Name = "ShortDescription")]
        public string ShortDescription { get; set; }
        [Required]
        [Display(Name = "Body")]
        public string Body { get; set; }
        [Required]
        [Display(Name = "Meta")]
        public string Meta { get; set; }
        [Required]
        [Display(Name = "UrlSeo")] // para sa url
        public string UrlSeo { get; set; }
        public bool Published { get; set; }  // kung na-publish - hindi na implement
        [DefaultValue(0)]
        public int NetLikeCount { get; set; }  // di naimplement
        public DateTime PostedOn { get; set; }  
        public DateTime? Modified { get; set; } 


        // Navigation properties which define the relationships between the tables

        public ICollection<Comment> Comments { get; set; }
        public ICollection<Reply> Replies { get; set; }
        public ICollection<PostCategory> PostCategories { get; set; }
        public ICollection<PostTag> PostTags { get; set; } // have link but di naimplement
        public ICollection<PostVideo> PostVideos { get; set; } // or image nalang
        public ICollection<PostLike> PostLikes { get; set; } // di naimplement

    }
    public class Category
    {
        public string Id { get; set; }
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }
        [Required]
        [Display(Name = "UrlSeo")]
        public string UrlSeo { get; set; }
        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }
        public bool Checked { get; set; }
        public ICollection<PostCategory> PostCategories { get; set; }
    }
    public class PostCategory
    {
        [Key]
        [Column(Order = 0)]
        public string PostId { get; set; }

        [Key]
        [Column(Order = 1)]
        public string CategoryId { get; set; }

        public bool Checked { get; set; }
        public Post Post { get; set; }
        public Category Category { get; set; }

    }
    public class Comment
    {
        public string Id { get; set; }
        public string PostId { get; set; }
        public DateTime DateTime { get; set; }
        public string UserName { get; set; }
        [Required]
        public string Body { get; set; }
        [DefaultValue(0)]
        public int NetLikeCount { get; set; }
        [DefaultValue(false)]
        public bool Deleted { get; set; }
        public Post Post { get; set; }
        public ICollection<Reply> Replies { get; set; }
        public ICollection<CommentLike> CommentLikes { get; set; }
    }
    public class Reply
    {
        public string Id { get; set; }
        public string PostId { get; set; }
        public string CommentId { get; set; }
        public string ParentReplyId { get; set; }
        public DateTime DateTime { get; set; }
        public string UserName { get; set; }
        [Required]
        public string Body { get; set; }
        [DefaultValue(false)]
        public bool Deleted { get; set; }
        public Post Post { get; set; }
        public Comment Comment { get; set; }
        public ICollection<ReplyLike> ReplyLikes { get; set; }
    }
    public class Tag
    {
        public string Id { get; set; }
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }
        [Required]
        [Display(Name = "UrlSeo")]
        public string UrlSeo { get; set; }

        public bool Checked { get; set; }
        public ICollection<PostTag> PostTags { get; set; }
    }
    public class PostTag
    {
        [Key]
        [Column(Order = 0)]
        public string PostId { get; set; }

        [Key]
        [Column(Order = 1)]
        public string TagId { get; set; }

        public bool Checked { get; set; }
        public Post Post { get; set; }
        public Tag Tag { get; set; }

    }
    public class PostVideo
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "VideoUrl")]
        public string VideoUrl { get; set; }
        public string VideoThumbnail { get; set; }
        public string PostId { get; set; }
        public string VideoSiteName { get; set; }

        public Post Post { get; set; }

    }
    public class PostLike
    {
        [Key]
        public string PostId { get; set; }
        public string Username { get; set; }
        public bool Like { get; set; }
        public bool Dislike { get; set; }

        public Post Post { get; set; }
    }
    public class CommentLike
    {
        [Key]
        public string CommentId { get; set; }
        public string Username { get; set; }
        public bool Like { get; set; }
        public bool Dislike { get; set; }
        public Comment Comment { get; set; }
    }
    public class ReplyLike
    {
        [Key]
        public string ReplyId { get; set; }
        public string Username { get; set; }
        public bool Like { get; set; }
        public bool Dislike { get; set; }
        public Reply Reply { get; set; }

    }
    public class BlogViewModel
    {
        public DateTime PostedOn { get; set; }
        public DateTime? Modified { get; set; }
        public IList<Tag> Tag { get; set; }
        public int PostDislikes { get; set; }
        public int PostLikes { get; set; }
        public int TotalPosts { get; set; }
        public List<string> Category { get; set; }
        public Post Post { get; set; }
        public string ID { get; set; }
        public string ShortDescription { get; set; }
        public string Title { get; set; }
        public IList<Category> PostCategories { get; set; }
        public IList<Tag> PostTags { get; set; }
        public string UrlSlug { get; set; }
    }
    public class AllPostsViewModel
    {
        public IList<Category> PostCategories { get; set; }
        public IList<Tag> PostTags { get; set; }
        public string PostId { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public Category Category { get; set; }
        public bool Checked { get; set; }
        public Tag Tag { get; set; }
        public string UrlSlug { get; set; }
    }
    public class PostViewModel
    {
        // add necessary properties to the viewmodel
        public string Body { get; set; }
        public string FirstPostId { get; set; }
        public string ID { get; set; }
        public string LastPostId { get; set; }
        public string NextPostSlug { get; set; }
        public int PostCount { get; set; }
        public int PostDislikes { get; set; }
        public int PostLikes { get; set; }
        public string PreviousPostSlug { get; set; }
        public string Title { get; set; }
        public IList<PostVideo> Videos { get; set; }
        public IList<Tag> PostTags { get; set; }
        public string Meta { get; set; }
        public string UrlSeo { get; set; }
        public IList<Category> PostCategories { get; set; }
        public string ShortDescription { get; set; }
        public IList<Category> Categories { get; set; }
        public IList<Tag> Tags { get; set; }

        public IList<Comment> Comments { get; set; }

    }
    public class CommentViewModel
    {
        public CommentViewModel() { }
        // For passing Comment's infos from Comments View to Replies view
        public CommentViewModel(Comment comment) 
        {
            Comment = comment;
        }
        public Comment Comment { get; set; }
        public DateTime DateTime { get; set; }
        public IList<CommentViewModel> ChildReplies { get; set; }
        public string Body { get; set; }
        public string Id { get; set; }
        public string ParentReplyId { get; set; }
        public string UserName { get; set; }
    }

}