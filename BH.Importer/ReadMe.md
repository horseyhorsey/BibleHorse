# BH.Importer
---

This is a BackgroundService but isn't supposed to be run as a background service.

Run this importer to convert a 2001HtmlDocument to sqlite database. See connection strings.

I found this was the best way to use existing services with dependency injection inside a console application.


See the `BH.Infrastructure.Parser_2001.cs` for scraping the html document into books / verses.
