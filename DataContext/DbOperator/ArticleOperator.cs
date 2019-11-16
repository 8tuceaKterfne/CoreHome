﻿using DataContext.DbConfig;
using DataContext.ModelDbContext;
using DataContext.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataContext.DbOperator
{
    public class ArticleOperator : IDbOperator<Article>
    {
        private readonly DbConfigurator configurator;
        private readonly CommentOperator commentOperator;

        public ArticleOperator()
        {
            configurator = new DbConfigurator();
            commentOperator = new CommentOperator();
        }

        /// <summary>
        /// 添加文章
        /// </summary>
        /// <param name="article">文章对象</param>
        public void Add(Article article)
        {
            using ArticleDbContext context = configurator.CreateArticleDbContext();
            context.Article.Add(article);
            context.SaveChanges();
        }

        /// <summary>
        /// 删除文章
        /// </summary>
        /// <param name="id">文章id</param>
        public void Delete(string id)
        {
            using ArticleDbContext context = configurator.CreateArticleDbContext();
            context.Article.Remove(Find(id));
            context.SaveChanges();

            //删除此文章的所有评论
            List<Comment> comments = commentOperator.Find(i => i.ArticleID == id, 0, commentOperator.Count()).ToList();
            comments.ForEach(comment =>
            {
                if (comment.ArticleID == id)
                {
                    commentOperator.Delete(comment.CommentID);
                }
            });
        }

        /// <summary>
        /// 修改博客
        /// </summary>
        /// <param name="id">需要修改的博客ID</param>
        /// <param name="newArticle">修改后的博客</param>
        public void Modify(Article newArticle)
        {
            using ArticleDbContext context = configurator.CreateArticleDbContext();
            //将新实体的修改进行插入
            context.Article.Update(newArticle);
            context.SaveChanges();
        }

        /// <summary>
        /// 单个文章查找
        /// </summary>
        /// <param name="id">博客ID</param>
        /// <returns>文章对象</returns>
        public Article Find(string id)
        {
            using ArticleDbContext context = configurator.CreateArticleDbContext();
            Article article = context.Article.Single(i => i.ArticleID == id);
            article.Comments = commentOperator.Find(i => i.ArticleID == id, 0, commentOperator.Count());
            return article;
        }

        /// <summary>
        /// 指定范围查找
        /// </summary>
        /// <param name="index">查找起点</param>
        /// <param name="pageSize">页面展示内容数量</param>
        /// <returns>文章对象列表</returns>
        public List<Article> Find(Func<Article, bool> func, int index, int pageSize)
        {
            int limit = index * pageSize;
            using ArticleDbContext context = configurator.CreateArticleDbContext();
            int count = Count() - limit;
            return context.Article.OrderByDescending(i => i.ID).Where(func).Skip(limit).Take(count > 5 ? pageSize : count).ToList();
        }

        /// <summary>
        /// 文章数量统计
        /// </summary>
        /// <returns>文章数量</returns>
        public int Count()
        {
            using ArticleDbContext context = configurator.CreateArticleDbContext();
            return context.Article.Count();
        }

    }
}
