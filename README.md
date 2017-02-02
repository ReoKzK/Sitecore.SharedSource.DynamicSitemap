# Sitecore.SharedSource.DynamicSitemap
Sitecore sitemap module that tries to be as much flexible and configurable as possible.

It has two main assumptions:

- Flexible configuration

	Gives end user as many as possible configuration options. It is easier to edit Sitecore items than edit and deploy config files.

- Extensibility
	
	Code functionality should be extensible and overridable. It should be easy to extend or override functionality, without decompiling and copy-pasting a lot of code.

Module was based on the original Sitemap XML module
	
### Features:
- Include items based on Base Templates (a.k.a inherited templates)
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

###Building the project
This project includes a web project that can be published to the relevant version of Sitecore (refer to the Sitecore Nuget Reference)

- Install a fresh instance of Sitecore. SIM (Sitecore Instance Manager) is recommended.
- Update the _/App_Config/Include/Unicorn/Unicorn.CustomSerializationFolder.config_ to point to the _Unicorn-Serialization_ folder
```
<targetDataStore type="Rainbow.Storage.SerializationFileSystemDataStore, Rainbow">
	<patch:attribute name="physicalRootPath">C:\projects\Sitecore.SharedSource.DynamicSitemap\Unicorn-Serialization\$(configurationName)</patch:attribute>
</targetDataStore>
```
- Publish the web project to the newly installed version of Sitecore 
- Run Unicorn sync http://mysite.local/Unicorn.aspx
- Upload and Open the _DynamicSitemapPackageDefinition.xml_ using the Package Manager in the Sitecore shell
- Make changes to the package definition as needed. *Make sure DynamicSitemapPackageDefinition.xml is updated in source control*
- Generate and download ZIP
	
###Configuration:
- Dynamic Sitemap XML provides Sitecore.SharedSource.DynamicSitemap.config configuration file installed in /App_Config/Include directory.

####Setting that can be specified there:
- sitemapConfigurationOutputFolder - folder where will be generated sitemap xml files
- refreshRobotsFile (true or false) - indicates that robots.txt file will be updated with references to sitemap xml files
- xmlnsTpl - sitemap module schema used for the XML sitemap
- database - the database from which to pull items for generating the sitemap
- submitToSearchEngine - (true or false) determines whether the sitemap should be submitted to the search engines or not
