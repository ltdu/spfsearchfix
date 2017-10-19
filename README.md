# Fix SharePoint Foundation 2013 Search
----
**UPDATE: Microsoft released official update to fix the problem: [http://support.microsoft.com/kb/2760625](http://support.microsoft.com/kb/2760625).**
WSP file is now removed from downloads page, but you can still get source code itself.
----
This project contains highly experimental code to 'work-around' a problem with search web parts in SharePoint Foundation 2013 after it was broken by some CU (maybe KB2837628 or KB2850058) or SP1.
What I did was:
* Created three web parts for search box, results and refinement panel
* Each has corresponding OOB web part as a base
* Overrided problematic methods and removed code which was causing the error.
	* This involved ILSpy and original DLL
	* Lots of reflection because most of the methods are private or internal
* Create Delegate control to replace small search box 
* Web parts and delegate control are activated via feature at site collection level

**If you can wait for official fix from MS, then go away :)**

If not, you can do the following:
* Download WSP file or compile and build your own.
* Install it on the affected SharePoint installation
* Activate 'SPFSearchFix.WebParts' feature
	* This will enable small search box 
	* And add web part to web part gallery
* Go to your search center and replace original web parts with these altered ones

This solution +does not+ fix pages, where original web parts are hard coded. For example /_layouts/15/osssearchresults.aspx.

This work-around should be taken AS IT IS. It may help you, but it will have to be removed once official fix becomes available.

This solution would have been impossible without some code found on internet:
* http://www.simplygoodcode.com/2012/08/invoke-base-method-using-reflection/
* http://stackoverflow.com/questions/1565734/is-it-possible-to-set-private-property-via-reflection
* And others

As for the code itself - this is great example of how NOT to code. It breaks all good practices, coding standards, and is not for the weak-hearted. I didn't do any testing, so you will find problems. Fix them yourself. 

Good luck.
