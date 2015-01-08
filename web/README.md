angular-seed-modular
====================

<p>Angular seed project with the build made for a more modular source file organization. To me this makes more sense for organizing a large project so that all the files involved with a component live in the same folder.</p>

<p>For example you can organize your /source folder like so:</p>

<pre>
source
	component1
		component1.js
		component1.html
		component1.scss
	component2
		component2.js
		component2.html
		component2.scss
</pre>

Other things that implemented in the grunt build are:
<ul>
	<li>-Automatic sprite sheet building. You can reference them in html as class="icon-<image filename without extention>"</li>
	<li>-Compiling of .html templates that are located in the /source directory to be used in the app</li>
</ul>

Hope this helps people!
