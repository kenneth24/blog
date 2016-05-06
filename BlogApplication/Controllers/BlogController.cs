﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using BlogApplication.DAL;
using BlogApplication.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BlogApplication.Controllers
{
    public class BlogController : Controller
    {

        private IBlogRepository _blogRepository;
        public static List<BlogViewModel> postList = new List<BlogViewModel>();
        public static List<AllPostsViewModel> allPostsList = new List<AllPostsViewModel>();
        public static List<AllPostsViewModel> checkCatList = new List<AllPostsViewModel>();
        public static List<AllPostsViewModel> checkTagList = new List<AllPostsViewModel>();

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public BlogController()
        {
            _blogRepository = new BlogRepository(new BlogDbContext());
        }

        public BlogController(IBlogRepository blogRepository, ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            _blogRepository = blogRepository;
            UserManager = userManager;
            SignInManager = signInManager;
        }
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        #region Index

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Index(int? page, string sortOrder, string searchString, string[] searchCategory, string[] searchTag)
        {
            checkCatList.Clear();
            CreateCatAndTagList();
            Posts(page, sortOrder, searchString, searchCategory, searchTag);
            return View();
        }

        #endregion Index

        #region Posts/AllPosts

        [ChildActionOnly]
        public ActionResult Posts(int? page, string sortOrder, string searchString, string[] searchCategory, string[] searchTag) //tag di naimplement
        {
            postList.Clear();

            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentSearchString = searchString;
            ViewBag.CurrentSearchCategory = searchCategory;
            ViewBag.CurrentSearchTag = searchTag;
            ViewBag.DateSortParm = string.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewBag.TitleSortParm = sortOrder == "Title" ? "title_desc" : "Title";

            var posts = _blogRepository.GetPosts();
            foreach (var post in posts)
            {
                var postCategories = GetPostCategories(post);
                //var postTags = GetPostTags(post);
                var likes = _blogRepository.LikeDislikeCount("postlike", post.Id);
                var dislikes = _blogRepository.LikeDislikeCount("postdislike", post.Id);
                //postList.Add(new BlogViewModel() { Post = post, Modified = post.Modified, Title = post.Title, ShortDescription = post.ShortDescription, PostedOn = post.PostedOn, ID = post.Id, PostCategories = postCategories, UrlSlug = post.UrlSeo });
                postList.Add(new BlogViewModel() { Post = post, Modified = post.Modified, Title = post.Title, ShortDescription = post.ShortDescription, PostedOn = post.PostedOn, ID = post.Id, PostLikes = likes, PostDislikes = dislikes, PostCategories = postCategories, UrlSlug = post.UrlSeo });
            }

            if (searchString != null)
            {
                postList = postList.Where(x => x.Title.ToLower().Contains(searchString.ToLower())).ToList();
            }
            //search category
            if (searchCategory != null)
            {
                List<BlogViewModel> newlist = new List<BlogViewModel>();
                foreach (var catName in searchCategory)
                {
                    foreach (var item in postList)
                    {
                        if (item.PostCategories.Where(x => x.Name == catName).Any())
                        {
                            newlist.Add(item);
                        }
                    }
                    foreach (var item in checkCatList)
                    {
                        if (item.Category.Name == catName)
                        {
                            item.Checked = true;
                        }
                    }
                }
                postList = postList.Intersect(newlist).ToList();
            }
            // para sa sorting ng date or title
            switch (sortOrder)
            {
                case "date_desc":
                    postList = postList.OrderBy(x => x.PostedOn).ToList();
                    break;
                case "Title":
                    postList = postList.OrderBy(x => x.Title).ToList();
                    break;
                case "title_desc":
                    postList = postList.OrderByDescending(x => x.Title).ToList();
                    break;
                default:
                    postList = postList.OrderByDescending(x => x.PostedOn).ToList();
                    break;
            }
            int pageSize = 5; // Number of page
            int pageNumber = (page ?? 1);
            return PartialView("Posts", postList.ToPagedList(pageNumber, pageSize));
        }

        //allposts
        [HttpGet]
        [AllowAnonymous]
        public ActionResult AllPosts(int? page, string sortOrder, string searchString, string[] searchCategory, string[] searchTag)
        {
            allPostsList.Clear();
            checkCatList.Clear();

            // assign a current filter to viewbag
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentSearchString = searchString;
            ViewBag.CurrentSearchCategory = searchCategory;
            //ViewBag.CurrentSearchTag = searchTag;

            ViewBag.DateSortParm = string.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewBag.TitleSortParm = sortOrder == "Title" ? "title_desc" : "Title";

            //allpostpagelist
            //Get all posts and for each of them create an AllPostsViewModel object and add it to the AllPostsList.
            var posts = _blogRepository.GetPosts();
            foreach (var post in posts)
            {
                var postCategories = GetPostCategories(post);
                //var postTags = GetPostTags(post);
                allPostsList.Add(new AllPostsViewModel() { PostId = post.Id, Date = post.PostedOn, Description = post.ShortDescription, Title = post.Title, PostCategories = postCategories, UrlSlug = post.UrlSeo });
            }

            if (searchString != null)
            {
                allPostsList = allPostsList.Where(x => x.Title.ToLower().Contains(searchString.ToLower())).ToList();
            }

            CreateCatAndTagList();

            if (searchCategory != null)
            {
                List<AllPostsViewModel> newlist = new List<AllPostsViewModel>();
                foreach (var catName in searchCategory)
                {
                    foreach (var item in allPostsList)
                    {
                        if (item.PostCategories.Where(x => x.Name == catName).Any())
                        {
                            newlist.Add(item);
                        }
                    }
                    foreach (var item in checkCatList)
                    {
                        if (item.Category.Name == catName)
                        {
                            item.Checked = true;
                        }
                    }
                }
                allPostsList = allPostsList.Intersect(newlist).ToList();
            }

            allPostsList = allPostsList.OrderByDescending(x => x.Date).ToList();
            //Define how many posts you want to show on each page, and return to view with model
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View("AllPosts", allPostsList.ToPagedList(pageNumber, pageSize));

        }
        #endregion Posts/AllPosts

        #region Post

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Post(string sortOrder, string slug)
        {
            PostViewModel model = new PostViewModel();
            var posts = GetPosts();
            var postid = _blogRepository.GetPostIdBySlug(slug);
            var post = _blogRepository.GetPostById(postid); // get the post
            //var videos = GetPostVideos(post);


            // --pag may time pa, NEXT and PREV sa actual post
            var firstPostId = posts.OrderBy(i => i.PostedOn).First().Id;
            var lastPostId = posts.OrderBy(i => i.PostedOn).Last().Id;

            var nextId = posts.OrderBy(i => i.PostedOn).SkipWhile(i => i.Id != postid).Skip(1).Select(i => i.Id).FirstOrDefault();
            var previousId = posts.OrderBy(i => i.PostedOn).TakeWhile(i => i.Id != postid).Select(i => i.Id).LastOrDefault();

            //assign those values to the related model properties, and return to view by using that model
            model.FirstPostId = firstPostId;
            model.LastPostId = lastPostId;
            model.PreviousPostSlug = posts.Where(x => x.Id == previousId).Select(x => x.UrlSeo).FirstOrDefault();
            model.NextPostSlug = posts.Where(x => x.Id == nextId).Select(x => x.UrlSeo).FirstOrDefault();

            model.ID = post.Id;
            model.PostCount = posts.Count();
            model.UrlSeo = post.UrlSeo;
            model.Title = post.Title;
            model.Body = post.Body;

            //model.PostLikes = _blogRepository.LikeDislikeCount("postlike", post.Id);
            //model.PostDislikes = _blogRepository.LikeDislikeCount("postdislike", post.Id);

            Comments(model, post, sortOrder);
            return View(model);
        }

        // add buttons for users to vote/like your post
        //public ActionResult UpdatePostLike(string postid, string slug, string username, string likeordislike, string sortorder)
        //{
        //    _blogRepository.UpdatePostLike(postid, username, likeordislike);
        //    return RedirectToAction("Post", new { slug = slug, sortorder = sortorder });
        //}


        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult EditPost(string slug)
        {
            var model = CreatePostViewModel(slug);
            return View(model);
        }

        // if you dont use ValidateInput(false) in the action method or  AllowHtml
        // for the Body and ShortDescription properties in the ViewModel, you'll get
        // A potential dangerous Request.Form value  was detected exception.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult EditPost(PostViewModel model)
        {
            var post = _blogRepository.GetPostById(model.ID);
            post.Body = model.Body;
            post.Title = model.Title;
            post.Meta = model.Meta;
            post.UrlSeo = model.UrlSeo;
            post.ShortDescription = model.ShortDescription;
            post.Modified = DateTime.Now;
            _blogRepository.Save();

            return RedirectToAction("Post", new { slug = model.UrlSeo });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult AddVideoToPost(string postid, string slug)
        {
            PostViewModel model = new PostViewModel();
            model.ID = postid;
            model.UrlSeo = slug;
            return View(model);
        }
        
        //[Authorize(Roles = "Admin")]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult AddVideoToPost(string postid, string slug, string videoUrl)
        //{
        //    CreatePostViewModel(slug);
        //    _blogRepository.AddVideoToPost(postid, videoUrl);
        //    return RedirectToAction("EditPost", new { slug = slug });
        //}


        //[Authorize(Roles = "Admin")]
        //public ActionResult RemoveVideoFromPost(string slug, string postid, string videoUrl)
        //{
        //    CreatePostViewModel(slug);
        //    _blogRepository.RemoveVideoFromPost(postid, videoUrl);
        //    return RedirectToAction("EditPost", new { slug = slug });
        //}


        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult AddCategoryToPost(string postid)
        {
            PostViewModel model = new PostViewModel();
            model.ID = postid;
            model.Categories = _blogRepository.GetCategories();
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCategoryToPost(PostViewModel model)
        {
            var post = _blogRepository.GetPostById(model.ID);
            var postCats = _blogRepository.GetPostCategories(post);
            //get all categories assigned to the post, and create a list of their ids.
            List<string> pCatIds = new List<string>();
            foreach (var pCat in postCats)
            {
                pCatIds.Add(pCat.Id);
            }
            // get the categories that has checked value of true and create a list of their ids
            var newCats = model.Categories.Where(x => x.Checked == true).ToList();
            List<string> nCatIds = new List<string>();
            foreach (var pCat in newCats)
            {
                nCatIds.Add(pCat.Id);
            }
            // Compare two lists using SequenceEqual method
            if (!pCatIds.SequenceEqual(nCatIds))
            {
                // if they are not equal remove the old categories, and add the ones that are checked
                foreach (var pCat in postCats)
                {
                    _blogRepository.RemovePostCategories(model.ID, pCat.Id);
                }
                foreach (var cat in model.Categories)
                {
                    PostCategory postCategory = new PostCategory();
                    if (cat.Checked == true)
                    {
                        postCategory.PostId = model.ID;
                        postCategory.CategoryId = cat.Id;
                        postCategory.Checked = true;
                        _blogRepository.AddPostCategories(postCategory);
                    }
                }
                _blogRepository.Save();
            }
            return RedirectToAction("EditPost", new { slug = post.UrlSeo });
        }

        [Authorize(Roles = "Admin")]
        public ActionResult RemoveCategoryFromPost(string slug, string postid, string catName)
        {
            CreatePostViewModel(slug);
            _blogRepository.RemoveCategoryFromPost(postid, catName);
            return RedirectToAction("EditPost", new { slug = slug });
        }


        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult AddNewCategory(string postid, bool callfrompost)
        {
            if (postid != null && callfrompost)
            {
                PostViewModel model = new PostViewModel();
                model.ID = postid;
                return View(model);
            }
            else
            {
                return View();
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddNewCategory(string postid, string catName, string catUrlSeo, string catDesc)
        {
            if (postid != null)
            {
                _blogRepository.AddNewCategory(catName, catUrlSeo, catDesc);
                return RedirectToAction("AddCategoryToPost", new { postid = postid });
            }
            else
            {
                _blogRepository.AddNewCategory(catName, catUrlSeo, catDesc);
                return RedirectToAction("CategoriesAndTags", "Blog");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult CategoriesAndTags()
        {
            checkCatList.Clear();
            //checkTagList.Clear();
            CreateCatAndTagList();
            return View();
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveCatAndTag(string[] categoryNames, string[] tagNames)
        {
            if (categoryNames != null)
            {
                foreach (var catName in categoryNames)
                {
                    var category = _blogRepository.GetCategories().Where(x => x.Name == catName).FirstOrDefault();
                    _blogRepository.RemoveCategory(category);
                }
            }
            //if (tagNames != null)
            //{
            //    foreach (var tagName in tagNames)
            //    {
            //       // var tag = _blogRepository.GetTags().Where(x => x.Name == tagName).FirstOrDefault();
            //        //_blogRepository.RemoveTag(tag);
            //    }
            //}
            return RedirectToAction("CategoriesAndTags", "Blog");
        }



        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult DeletePost(PostViewModel model, string postid)
        {
            model.ID = postid;
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(string postId)
        {
            _blogRepository.DeletePostandComponents(postId);
            return RedirectToAction("Index", "Blog");
        }


        // if int ids, just sort them and get the next value of the last one
        // just make sure id's unique
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult AddNewPost()
        {
            List<int> numlist = new List<int>();
            int num = 0;
            var posts = _blogRepository.GetPosts();
            if (posts.Count != 0)
            {
                foreach (var post in posts)
                {
                    var postid = post.Id;
                    Int32.TryParse(postid, out num);
                    numlist.Add(num);
                }
                numlist.Sort();
                num = numlist.Last();
                num++;
            }
            else
            {
                num = 1;
            }
            var newid = num.ToString();
            PostViewModel model = new PostViewModel();
            model.ID = newid;
            return View(model);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult AddNewPost(PostViewModel model)
        {
            var post = new Post
            {
                Id = model.ID,
                Body = model.Body,
                Meta = model.Meta,
                PostedOn = DateTime.Now,
                Published = true,
                ShortDescription = model.ShortDescription,
                Title = model.Title,
                UrlSeo = model.UrlSeo
            };
            _blogRepository.AddNewPost(post);
            return RedirectToAction("EditPost", "Blog", new { slug = model.UrlSeo });
        }


        #endregion Post

        #region Rss

        //public ActionResult Feed()
        //{
        //    var blogTitle = "Your Blog Title";
        //    var blogDescription = " Your Blog Description.";
        //    var blogUrl = "http://yourblog.com ";

        //    var posts = _blogRepository.GetPosts().Select(
        //        p => new SyndicationItem(
        //            p.Title,
        //            p.ShortDescription,
        //          new Uri(blogUrl)
        //        )
        //        );

        //    // Create an instance of SyndicationFeed class passing the SyndicationItem collection
        //    var feed = new SyndicationFeed(blogTitle, blogDescription, new Uri(blogUrl), posts)
        //    {
        //        Copyright = new TextSyndicationContent(string.Format("Copyright © {0}", blogTitle)),
        //        Language = "en-US"
        //    };

        //    // Format feed in RSS format through Rss20FeedFormatter formatter
        //    var feedFormatter = new Rss20FeedFormatter(feed);

        //    // Call the custom action that write the feed to the response
        //    return new FeedResult(feedFormatter);

        //}

        #endregion Rss

        [ChildActionOnly]
        public ActionResult Comments(PostViewModel model, Post post, string sortOrder)
        {
            // for sorting comments by date
            ViewBag.CurrentSort = sortOrder;
            ViewBag.DateSortParm = string.IsNullOrEmpty(sortOrder) ? "date_asc" : "";
            ViewBag.BestSortParm = sortOrder == "Best" ? "best_desc" : "Best";

            // get list of posts comment
            var postComments = _blogRepository.GetPostComments(post).OrderByDescending(d => d.DateTime).ToList();

            foreach (var comment in postComments)
            {
                var likes = LikeDislikeCount("commentlike", comment.Id);
                var dislikes = LikeDislikeCount("commentdislike", comment.Id);
                comment.NetLikeCount = likes - dislikes;
                // for each comment, clear its replies since we only want the comment and not the child replies
                if (comment.Replies != null) comment.Replies.Clear();

                // create a list of parent replies. And because we need them as reply and not as CommentViewModel object
                // using their id's get them as Reply object and add them to the comment's replies
                List<CommentViewModel> replies = _blogRepository.GetParentReplies(comment);
                foreach (var reply in replies)
                {
                    var rep = _blogRepository.GetReplyById(reply.Id);
                    comment.Replies.Add(rep);
                }
            }

            //sorting comment based on user's click. active is for css to change the color of chosen sort link
            switch (sortOrder)
            {
                case "date_asc":
                    postComments = postComments.OrderBy(x => x.DateTime).ToList();
                    ViewBag.DateSortLink = "active";
                    break;
                case "Best":
                    postComments = postComments.OrderByDescending(x => x.NetLikeCount).ToList();
                    ViewBag.BestSortLink = "active";
                    break;
                case "best_desc":
                    postComments = postComments.OrderBy(x => x.NetLikeCount).ToList();
                    ViewBag.BestSortLink = "active";
                    break;
                default:
                    postComments = postComments.OrderByDescending(x => x.DateTime).ToList();
                    ViewBag.DateSortLink = "active";
                    break;
            }

            model.UrlSeo = post.UrlSeo;
            model.Comments = postComments;
            return PartialView(model);
        }

        public PartialViewResult Replies()
        {
            return PartialView();
        }

        public PartialViewResult ChildReplies()
        {
            return PartialView();
        }


        public ActionResult UpdateCommentLike(string commentid, string username, string likeordislike, string slug, string sortorder)
        {
            if (username != null)
            {
                _blogRepository.UpdateCommentLike(commentid, username, likeordislike);
            }
            return RedirectToAction("Post", new { slug = slug, sortorder = sortorder });
        }
        public ActionResult UpdateReplyLike(string replyid, string username, string likeordislike, string sortorder)
        {
            if (username != null)
            {
                _blogRepository.UpdateReplyLike(replyid, username, likeordislike);
            }
            var slug = _blogRepository.GetPostByReply(replyid).UrlSeo;
            return RedirectToAction("Post", "Blog", new { slug = slug, sortorder = sortorder });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult NewComment(string commentBody, string comUserName, string slug, string postid)
        {
            List<int> numlist = new List<int>();
            // create a unique id for comment
            int num = 0;
            var comments = _blogRepository.GetComments().ToList();
            if (comments.Count() != 0)
            {
                foreach (var cmnt in comments)
                {
                    var comid = cmnt.Id;
                    Int32.TryParse(comid.Replace("cmt", ""), out num);
                    numlist.Add(num);
                }
                numlist.Sort();
                num = numlist.Last();
                num++;
            }
            else
            {
                num = 1;
            }
            var newid = "cmt" + num.ToString();

            // create a new comment object and add it to the database
            var comment = new Comment()
            {
                Id = newid,
                PostId = postid,
                DateTime = DateTime.Now,
                UserName = comUserName,
                Body = commentBody,
                NetLikeCount = 0
            };
            _blogRepository.AddNewComment(comment);
            return RedirectToAction("Post", new { slug = slug });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult NewParentReply(string replyBody, string comUserName, string postid, string commentid, string slug)
        {
            var comDelChck = CommentDeleteCheck(commentid);
            if (!comDelChck)
            {
                // create a unieque id for reply
                List<int> numlist = new List<int>();
                int num = 0;
                var replies = _blogRepository.GetReplies().ToList();
                if (replies.Count != 0)
                {
                    foreach (var rep in replies)
                    {
                        var repid = rep.Id;
                        Int32.TryParse(repid.Replace("rep", ""), out num);
                        numlist.Add(num);
                    }
                    numlist.Sort();
                    num = numlist.Last();
                    num++;
                }
                else
                {
                    num = 1;
                }
                var newid = "rep" + num.ToString();

                // create a new reply object and add it to the database
                var reply = new Reply()
                {
                    Id = newid,
                    PostId = postid,
                    CommentId = commentid,
                    ParentReplyId = null,
                    DateTime = DateTime.Now,
                    UserName = comUserName,
                    Body = replyBody,
                };
                _blogRepository.AddNewReply(reply);
            }
            return RedirectToAction("Post", new { slug = slug });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult NewChildReply(string preplyid, string comUserName, string replyBody)
        {
            var repDelCheck = ReplyDeleteCheck(preplyid);
            var preply = _blogRepository.GetReplyById(preplyid);
            if (!repDelCheck)
            {
                List<int> numlist = new List<int>();
                int num = 0;
                var replies = _blogRepository.GetReplies().ToList();
                if (replies.Count != 0)
                {
                    foreach (var rep in replies)
                    {
                        var repid = rep.Id;
                        Int32.TryParse(repid.Replace("rep", ""), out num);
                        numlist.Add(num);
                    }
                    numlist.Sort();
                    num = numlist.Last();
                    num++;
                }
                else
                {
                    num = 1;
                }
                var newid = "rep" + num.ToString();
                var reply = new Reply()
                {
                    Id = newid,
                    PostId = preply.PostId,
                    CommentId = preply.CommentId,
                    ParentReplyId = preply.Id,
                    DateTime = DateTime.Now,
                    UserName = comUserName,
                    Body = replyBody,
                };
                _blogRepository.AddNewReply(reply);
            }
            return RedirectToAction("Post", new { slug = _blogRepository.GetPosts().Where(x => x.Id == preply.PostId).FirstOrDefault().UrlSeo });
        }



        [HttpGet]
        public async Task<ActionResult> EditComment(CommentViewModel model, string commentid)
        {
            var user = await GetCurrentUserAsync();
            var comment = _blogRepository.GetCommentById(commentid);

            // if COMMENT BELONGS TO THE SIGNED IN USER GO TO EDITCOMMENT VIEW ELSE RETURN TO POST VIEW
            if (comment.UserName == user.UserName)
            {
                model.Id = commentid;
                model.Body = comment.Body;
                return View(model);
            }
            else
            {
                return RedirectToAction("Post", new { slug = _blogRepository.GetPosts().Where(x => x.Id == comment.PostId).FirstOrDefault().UrlSeo });
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult EditComment(string commentid, string commentBody)
        {
            var comment = _blogRepository.GetCommentById(commentid);
            comment.Body = commentBody;
            comment.DateTime = DateTime.Now;
            _blogRepository.Save();
            return RedirectToAction("Post", new { slug = _blogRepository.GetPosts().Where(x => x.Id == comment.PostId).FirstOrDefault().UrlSeo });
        }


        [HttpGet]
        public async Task<ActionResult> DeleteComment(CommentViewModel model, string commentid)
        {
            var user = await GetCurrentUserAsync();
            var comment = _blogRepository.GetCommentById(commentid);
            if (comment.UserName == user.UserName)
            {
                model.Id = commentid;
                return View(model);
            }
            else
            {
                return RedirectToAction("Post", new { slug = _blogRepository.GetPosts().Where(x => x.Id == comment.PostId).FirstOrDefault().UrlSeo });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteComment(string commentid)
        {
            var comment = _blogRepository.GetCommentById(commentid);
            var postid = comment.PostId;
            var repliesList = _blogRepository.GetParentReplies(comment);
            if (repliesList.Count() == 0)
            {
                _blogRepository.DeleteComment(commentid);
            }
            else
            {
                comment.DateTime = DateTime.Now;
                comment.Body = "<p style=\"color:red;\"><i>This comment has been deleted.</i></p>";
                comment.Deleted = true;
                _blogRepository.Save();
            }
            return RedirectToAction("Post", new { slug = _blogRepository.GetPosts().Where(x => x.Id == postid).FirstOrDefault().UrlSeo });
        }


        [HttpGet]
        public async Task<ActionResult> EditReply(CommentViewModel model, string replyid)
        {
            var user = await GetCurrentUserAsync();
            var reply = _blogRepository.GetReplyById(replyid);
            if (reply.UserName == user.UserName)
            {
                model.Id = replyid;
                model.Body = reply.Body;
                return View(model);
            }
            else
            {
                return RedirectToAction("Post", new { slug = _blogRepository.GetPosts().Where(x => x.Id == reply.PostId).FirstOrDefault().UrlSeo });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult EditReply(string replyid, string replyBody)
        {
            var reply = _blogRepository.GetReplyById(replyid);
            reply.Body = replyBody;
            reply.DateTime = DateTime.Now;
            _blogRepository.Save();
            return RedirectToAction("Post", new { slug = _blogRepository.GetPosts().Where(x => x.Id == reply.PostId).FirstOrDefault().UrlSeo });
        }


        [HttpGet]
        public async Task<ActionResult> DeleteReply(CommentViewModel model, string replyid)
        {
            var user = await GetCurrentUserAsync();
            var reply = _blogRepository.GetReplyById(replyid);
            if (reply.UserName == user.UserName)
            {
                model.Id = replyid;
                return View(model);
            }
            else
            {
                return RedirectToAction("Post", new { slug = _blogRepository.GetPosts().Where(x => x.Id == reply.PostId).FirstOrDefault().UrlSeo });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteReply(string replyid)
        {
            var reply = _blogRepository.GetReplyById(replyid);
            var repliesList = _blogRepository.GetChildReplies(reply);
            var postid = reply.PostId;
            // if comment has no replies.
            if (repliesList.Count() == 0)
            {
                _blogRepository.DeleteReply(replyid);
            }
            else
            {
                reply.DateTime = DateTime.Now;
                reply.Body = "<p style=\"color:red;\"><i>This comment has been deleted.</i></p>";
                reply.Deleted = true;
                _blogRepository.Save();
            }
            return RedirectToAction("Post", new { slug = _blogRepository.GetPosts().Where(x => x.Id == postid).FirstOrDefault().UrlSeo });
        }





        #region Helpers

        // for getting current user
        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            return await UserManager.FindByIdAsync(User.Identity.GetUserId());
        }


        public List<CommentViewModel> GetChildReplies(Reply parentReply)
        {
            return _blogRepository.GetChildReplies(parentReply);
        }


        public bool CommentDeleteCheck(string commentid)
        {
            return _blogRepository.CommentDeleteCheck(commentid);
        }
        public bool ReplyDeleteCheck(string replyid)
        {
            return _blogRepository.ReplyDeleteCheck(replyid);
        }



        public static string TimePassed(DateTime postDate)
        {
            string date = null;
            double dateDiff = 0.0;
            var timeDiff = DateTime.Now - postDate;
            var yearPassed = timeDiff.TotalDays / 365;
            var monthPassed = timeDiff.TotalDays / 30;
            var dayPassed = timeDiff.TotalDays;
            var hourPassed = timeDiff.TotalHours;
            var minutePassed = timeDiff.TotalMinutes;
            var secondPassed = timeDiff.TotalSeconds;
            if (Math.Floor(yearPassed) > 0)
            {
                dateDiff = Math.Floor(yearPassed);
                date = dateDiff == 1 ? dateDiff + " year ago" : dateDiff + " years ago";
            }
            else
            {
                if (Math.Floor(monthPassed) > 0)
                {
                    dateDiff = Math.Floor(monthPassed);
                    date = dateDiff == 1 ? dateDiff + " month ago" : dateDiff + " months ago";
                }
                else
                {
                    if (Math.Floor(dayPassed) > 0)
                    {
                        dateDiff = Math.Floor(dayPassed);
                        date = dateDiff == 1 ? dateDiff + " day ago" : dateDiff + " days ago";
                    }
                    else
                    {
                        if (Math.Floor(hourPassed) > 0)
                        {
                            dateDiff = Math.Floor(hourPassed);
                            date = dateDiff == 1 ? dateDiff + " hour ago" : dateDiff + " hours ago";
                        }
                        else
                        {
                            if (Math.Floor(minutePassed) > 0)
                            {
                                dateDiff = Math.Floor(minutePassed);
                                date = dateDiff == 1 ? dateDiff + " minute ago" : dateDiff + " minutes ago";
                            }
                            else
                            {
                                dateDiff = Math.Floor(secondPassed);
                                date = dateDiff == 1 ? dateDiff + " second ago" : dateDiff + " seconds ago";
                            }
                        }
                    }
                }
            }

            return date;
        }


        public string[] NewCommentDetails(string username)
        {
            string[] newCommentDetails = new string[3];
            newCommentDetails[0] = "td" + username; //comText
            newCommentDetails[1] = "tdc" + username; //comTextdiv
            newCommentDetails[2] = "tb" + username; //comTextBtn
            return newCommentDetails;
        }

        //arrays for adding some effects using javascript
        public string[] CommentDetails(Comment comment)
        {
            string[] commentDetails = new string[17];
            commentDetails[0] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(comment.UserName);//username
            commentDetails[1] = "/Content/images/profile/" + commentDetails[0] + ".png?time=" + DateTime.Now.ToString();//imgUrl
            commentDetails[2] = TimePassed(comment.DateTime);//passed time
            commentDetails[3] = comment.DateTime.ToLongDateString().Replace(comment.DateTime.DayOfWeek.ToString() + ", ", "");//comment date
            commentDetails[4] = "gp" + comment.Id; //grandparentid
            commentDetails[5] = "mc" + comment.Id; //maincommentId
            commentDetails[6] = "crp" + comment.Id; //repliesId
            commentDetails[7] = "cex" + comment.Id; //commentExpid
            commentDetails[8] = "ctex" + comment.Id; //ctrlExpId
            commentDetails[9] = "ctflg" + comment.Id; //ctrlFlagId
            commentDetails[10] = "sp" + comment.Id; //shareParentId
            commentDetails[11] = "sc" + comment.Id; //shareChildId
            commentDetails[12] = "td" + comment.Id; //comText
            commentDetails[13] = "tdc" + comment.Id; //comTextdiv
            commentDetails[14] = "rpl" + comment.Id; //Reply
            commentDetails[15] = "cc1" + comment.Id; //commentControl
            commentDetails[16] = "cc2" + comment.Id; //commentMenu
            return commentDetails;
        }

        public string[] ReplyDetails(string replyId)
        {
            string[] replyDetails = new string[17];
            var reply = _blogRepository.GetReplyById(replyId);
            replyDetails[0] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(reply.UserName);//username
            replyDetails[1] = "/Content/images/profile/" + replyDetails[0] + ".png?time=" + DateTime.Now.ToString(); //imgUrl
            replyDetails[2] = TimePassed(reply.DateTime); //passed time
            replyDetails[3] = reply.DateTime.ToLongDateString().Replace(reply.DateTime.DayOfWeek.ToString() + ", ", ""); //reply date
            replyDetails[4] = "gp" + replyId; //grandparentid
            replyDetails[5] = "rp" + replyId; //parentreplyId
            replyDetails[6] = "crp" + replyId; //repliesId
            replyDetails[7] = "cex" + replyId; //commentExpid
            replyDetails[8] = "ctex" + replyId; //ctrlExpId
            replyDetails[9] = "ctflg" + replyId; //ctrlFlagId
            replyDetails[10] = "sp" + replyId; //shareParentId
            replyDetails[11] = "sc" + replyId; //shareChildId;
            replyDetails[12] = "td" + replyId; //comText
            replyDetails[13] = "tdc" + replyId; //comTextdiv
            replyDetails[14] = "rpl" + replyId; //Reply
            replyDetails[15] = "cc1" + replyId; //commentControl
            replyDetails[16] = "cc2" + replyId; //commentMenu

            return replyDetails;
        }

        public int LikeDislikeCount(string typeAndlike, string id)
        {
            switch (typeAndlike)
            {
                case "commentlike":
                    return _blogRepository.LikeDislikeCount("commentlike", id);
                case "commentdislike":
                    return _blogRepository.LikeDislikeCount("commentdislike", id);
                case "replylike":
                    return _blogRepository.LikeDislikeCount("replylike", id);
                case "replydislike":
                    return _blogRepository.LikeDislikeCount("replydislike", id);
                default:
                    return 0;
            }
        }



        public IList<Post> GetPosts()
        {
            return _blogRepository.GetPosts();
        }
        public IList<Category> GetPostCategories(Post post)
        {
            return _blogRepository.GetPostCategories(post);
        }


        //public IList<Tag> GetPostTags(Post post)
        //{
        //    return _blogRepository.GetPostTags(post);
        //}
        //public IList<PostVideo> GetPostVideos(Post post)
        //{
        //    return _blogRepository.GetPostVideos(post);
        //}

        public void CreateCatAndTagList()
        {
            foreach (var ct in _blogRepository.GetCategories())
            {
                checkCatList.Add(new AllPostsViewModel { Category = ct, Checked = false });
            }
            //foreach (var tg in _blogRepository.GetTags())
            //{
            //    checkTagList.Add(new AllPostsViewModel { Tag = tg, Checked = false });
            //}
        }
        // helper method to create a PostViewModel using post's URL Slug
        public PostViewModel CreatePostViewModel(string slug)
        {
            PostViewModel model = new PostViewModel();
            var postid = _blogRepository.GetPostIdBySlug(slug);
            var post = _blogRepository.GetPostById(postid);
            model.ID = postid;
            model.Title = post.Title;
            model.Body = post.Body;
            model.Meta = post.Meta;
            model.UrlSeo = post.UrlSeo;
            //model.Videos = _blogRepository.GetPostVideos(post).ToList();
            model.PostCategories = _blogRepository.GetPostCategories(post).ToList();
            //model.PostTags = _blogRepository.GetPostTags(post).ToList();
            model.ShortDescription = post.ShortDescription;
            return model;
        }


        #endregion
    }
}