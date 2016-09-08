# Sitecore.SharedSource.DynamicSitemap
Sitecore sitemap module that tries to be as much flexible and configurable as possible.

It has two main assumptions:

- Flexible configuration

	Gives end user as many as possible configuration options. It is easier to edit Sitecore items than edit and deploy config files.

- Extensibility
	
	Code functionality should be extensible and overridable. It should be easy to extend or override functionality, without decompiling and copy-pasting a lot of code.

-

Module was based on the original Sitemap XML module
	
###New features:
- Configurations for sites controlled from Content Editor
- Defining <changefreq> and <priority> tags for templates and specified items
- Language fallback for configuration items
- Overridable method ProcessItems() for specifing own low-level logic
- Generating sitemap index file
	
###Original Sitemap XML features
- Multisite and multilanguage support
- Automatically updated robots.txt file
- Physical XML sitemaps files generation
- Automatic sitemap submit on publish
	
	
###Installation:
- Install zip package using Sitecore Package Installer
	
###Configuration:
- Dynamic Sitemap XML provides Sitecore.SharedSource.DynamicSitemap.config configuration file installed in /App_Config/Include directory.

####Setting that can be specified there:
- sitemapConfigurationOutputFolder - folder where will be generated sitemap xml files
- refreshRobotsFile (true or false) - indicates that robots.txt file will be updated with references to sitemap xml files
- xmlnsTpl - sitemap module schema used for the XML sitemap
- database - the database from which to pull items for generating the sitemap
- productionEnvironment - (true or false) determines whether the sitemap should be submitted to the search engines or not
