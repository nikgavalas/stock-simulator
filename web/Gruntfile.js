// Generated on 2013-09-30 using generator-angular 0.4.0
'use strict';

/*jshint camelcase:false*/
var LOCALHOST_PORT = 9000;
var LIVERELOAD_PORT = 35729;
var lrSnippet = require('connect-livereload')({ port: LIVERELOAD_PORT });
var mountFolder = function (connect, dir) {
	return connect.static(require('path').resolve(dir));
};

// Sets the headers for any requests to the server to allow for cross domain
// ajax requests. This only applies to the nodejs test server. .htaccess is
// modified as well.
var corsMiddleware = function(req, res, next) {
	res.setHeader('Access-Control-Allow-Origin', '*');
	res.setHeader('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
	res.setHeader('Access-Control-Allow-Headers', '*');
	next();
};

/**
 * Main grunt export
 * @param  {Object} grunt Grunt Object
 */
module.exports = function (grunt) {
	require('load-grunt-tasks')(grunt);
	require('time-grunt')(grunt);

	var exec = require('child_process').exec;

	// configurable paths
	var yeomanConfig = {
		root: 'project',
		build: 'build',
		temp: 'project/.temp'
	};

	// In case in bower.json there is another path specified.
	try {
		var bowerJson = require('./bower.json');
		yeomanConfig.root = bowerJson.appPath || yeomanConfig.root;
	} catch (e) {}


	// TODO: fix this path.
	var isDebug = grunt.option('dbg') !== undefined;
	if (isDebug) {
		yeomanConfig.build = 'build-debug';
	}


	var gruntConfig = {
		yeoman: yeomanConfig,

		/**
		 * Task that cleans our built and temp folders.
		 */
		clean: {
			dist: {
				files: [{
					dot: true,
					src: [
						'<%= yeoman.build %>/**/*',
						'<%= yeoman.temp %>/**/*',
					]
				}]
			},
			server: '<%= yeoman.temp %>/**/*'
		},

		/**
		 * Task to build all the scss files.
		 */
		sass: {
			options: {
				loadPath: [
					'<%= yeoman.root %>/'
				]
			},
			main: {
				options: {
					style: 'expanded'
				},
				files: {
					'<%= yeoman.temp %>/styles/main.css': '<%= yeoman.root %>/source/main.scss'
				}
			}
		},

		/**
		 * Copys files to the correct places depending on the target.
		 */
		copy: {
			fonts: {
				files: [
					// Copy fonts
					{
						expand: true,
						dot: true,
						cwd: '<%= yeoman.root %>',
						dest: '<%= yeoman.build %>',
						src: [
							'fonts/**/*.{eot,svg,ttf,woff}'
						]
					}
				]				
			},

			bootstrapFontsServer: {
				expand: true,
				cwd: '<%= yeoman.root %>/bower-components/bootstrap-sass-official/assets/fonts',
				dest: '<%= yeoman.temp %>/fonts',
				src: '**/*.{eot,svg,ttf,woff}'
			},
			bootstrapFontsDist: {
				expand: true,
				cwd: '<%= yeoman.root %>/bower-components/bootstrap-sass-official/assets/fonts',
				dest: '<%= yeoman.build %>/fonts',
				src: '**/*.{eot,svg,ttf,woff}'
			},

			mainDist: {
				files: [
					// Copy all the misc files that are in the root admin directory.
					{
						expand: true,
						cwd: '<%= yeoman.root %>',
						dest: '<%= yeoman.build %>',
						src: [
							'*.{ico,png,txt}',
							'.htaccess'
						]
					},
					// Copy the spritesmith generated images
					{
						expand: true,
						cwd: '<%= yeoman.temp %>/images',
						dest: '<%= yeoman.build %>/images',
						src: [
							'*.{png,jpg,jpeg,gif,webp,svg}'
						]
					}
				]
			},			

		},

		/**
		 * Opens a local nodejs server for rapid prototyping.
		 */
		connect: {
			options: {
				port: LOCALHOST_PORT,
				// Change this to '0.0.0.0' to access the server from outside.
				hostname: 'localhost'
			},
			livereload: {
				options: {
					middleware: function (connect) {
						return [
							lrSnippet,
							mountFolder(connect, yeomanConfig.temp),
							mountFolder(connect, yeomanConfig.root),
							mountFolder(connect, '../output'),
							corsMiddleware
						];
					}
				}
			},
			dist: {
				options: {
					middleware: function (connect) {
						return [
							mountFolder(connect, yeomanConfig.build),
							mountFolder(connect, '../output'),
						];
					}
				}
			}
		},

		/**
		 * Opens the browser to our local node webserver.
		 */
		open: {
			server: {
				url: 'http://localhost:<%= connect.options.port %>/#/test'
			}
		},

		/**
		 * Watches any files for changes and runs the task to build and/or reload them.
		 */
		watch: {
			mainSass: {
				files: ['project/source/**/*.scss', 'project/fonts/**/*.scss'],
				tasks: [
					'sass:main'
				]
			},
			mainTemplates: {
				files: ['project/source/**/*.html'],
				tasks: [
					'html2js:main'
				]
			},
			mainImages: {
				files: ['project/images/**/*.{png,jpg,jpeg,gif,webp,svg}'],
				tasks: [
					'sprite:main',
					'sass:main'
				]
			},

			//
			// Live reload for the node server
			// 
			livereload: {
				options: {
					livereload: LIVERELOAD_PORT
				},
				files: [
					'<%= yeoman.root %>/**/index.html',
					'<%= yeoman.temp %>/**/*.css',
					'<%= yeoman.temp %>/**/templates.js',

					// Main
					'<%= yeoman.root %>/source/**/*.js',
					'<%= yeoman.root %>/source/**/*.{png,jpg,jpeg,gif,webp,svg}',
					'<%= yeoman.root %>/views/**/*.html'
				]
			}
		},

		/**
		 * Converts html files as templates to a js file that can be included as an angular module.
		 */
		html2js: {
			main: {
				options: {
					quoteChar: '\'',
					useStrict: true,
					base: '<%= yeoman.root %>/source',
					module: 'templates-mainApp',
					rename: function(moduleName) {
						return 'source/' + moduleName;
					}
				},
				files: [{
					src: [
						'<%= yeoman.root %>/source/**/*.html'
					],
					dest: '<%= yeoman.temp %>/scripts/templates.js'
				}]
			}			
		},

		/**
		 * Builds all the images into a spritesheet for more efficient loading.
		 */
		sprite:{
			main: {
				src: '<%= yeoman.root %>/images/**/*.{png,jpg,jpeg,gif,webp,svg}',
				destImg: '<%= yeoman.temp %>/images/sprites.png',
				destCSS: '<%= yeoman.temp %>/styles/sprites.css'
			}
		},

		htmlmin: {
			options: {
				/*removeCommentsFromCDATA: true,
				// https://github.com/yeoman/grunt-usemin/issues/44
				//collapseWhitespace: true,
				collapseBooleanAttributes: true,
				removeAttributeQuotes: true,
				removeRedundantAttributes: true,
				useShortDoctype: true,
				removeEmptyAttributes: true,
				removeOptionalTags: true*/
			},
			main: {
				files: [{
					expand: true,
					cwd: '<%= yeoman.root %>',
					src: ['index.html', '404.html', 'views/**/*.html'],
					dest: '<%= yeoman.build %>'
				}]
			}			
		},

		/**
		 * Preps the index file to use the minified versions of the files.
		 */
		useminPrepare: {
			main: {
				src: '<%= yeoman.root %>/index.html',
				options: {
					dest: '<%= yeoman.build %>',
					staging: '<%= yeoman.temp %>/staging'
				}
			}
		},

		/**
		 * Actually replace the references to the files with the hashed/minified versions
		 */
		usemin: {
			'main-html': {
				options: {
					type: 'html',
					assetsDirs: [
						'<%= yeoman.build %>'
					]
				},
				files: { 
					src: [
						'<%= yeoman.build %>/*.html',
					]
				}
			},
			'main-css': {
				options: {
					type: 'css',
					assetsDirs: [
						'<%= yeoman.build %>',
						'<%= yeoman.build %>/styles/**/',
						'<%= yeoman.build %>/images'
					]
				},
				files: { src: ['<%= yeoman.build %>/styles/**/*.css'] }
			},
			'main-js': {
				options: {
					type: 'js',
					assetsDirs: [
						'<%= yeoman.build %>',
						'<%= yeoman.build %>/scripts',
						'<%= yeoman.build %>/images'
					],
					patterns: {
						'main-js': []
					}
				},
				files: { src: ['<%= yeoman.build %>/scripts/*.js'] }
			}
		},

		/**
		 * Task is here so that the options can be modified. The useminPrepare task
		 * actually populates the config properties for this task object.
		 */
		uglify: {
			options: {},
		},

		/**
		 * Generates a file hash based on the content and prepends it to the name to invalidate
		 * the cached version that the browser may keep.
		 */
		rev: {
			main: {
				files: {
					src: [
						'<%= yeoman.build %>/scripts/**/*.js',
						'<%= yeoman.build %>/styles/**/*.css',
						'<%= yeoman.build %>/images/**/*.{png,jpg,jpeg,gif,webp,svg}'//,
					]
				}
			}
		},

		jshint: {
			options: {
				jshintrc: '.jshintrc'
			},
			all: [
				'Gruntfile.js',
				'<%= yeoman.root %>/source/**/*.js',

				// Excluded files example
				//'!<%= yeoman.root %>/common/source/wicket/*.js',
			]
		},

	};


	if (isDebug) {
		gruntConfig.uglify.options = { 
			mangle: false,
			compress: false,
			beautify: true,
			preserveComments: true
		};
	}

	// Setup a preprocess flag to use for release only code.
	if (grunt.cli.tasks) {
		var isReleaseBuild = true;
		for (var i = 0; i < grunt.cli.tasks.length; i++) {
			if (grunt.cli.tasks[i] === 'server') {
				isReleaseBuild = false;
				break;
			}
		}

		if (isReleaseBuild) {
			gruntConfig.uglify.options.compress = { 
				global_defs: {
					RELEASE_BUILD: true
				}
			};
		}
	}



	grunt.initConfig(gruntConfig);

	grunt.registerTask('task', function() {
		grunt.task.run([
			'usemin:launcher-js'
		]);
	});

	grunt.registerTask('server', function (target) {
		if (target === 'dist') {
			return grunt.task.run(['open', 'connect:dist:keepalive']);
		}

		grunt.task.run([
			'clean:server',

			// Common folder tasks
			'copy:bootstrapFontsServer',

			// Main specific tasks
			'sprite:main',
			'sass:main',
			'html2js:main',

			// Start the watches and open the server.
			'connect:livereload',
			'open',
			'watch'
		]);
	});

	grunt.registerTask('build', function() {

		// exec('hg id -i', function(err, stdout, stderr){
		// 	if (err) {
		// 		grunt.fatal('Can not get a version number using \'hg id -i\'');
		// 	}
		// 	var hgRevision = stdout.trim();

		// 	grunt.file.write(yeomanConfig.build + '/version.json', JSON.stringify({
		// 		version: bowerJson.version || '0.0.0',
		// 		revision: hgRevision,
		// 		date: grunt.template.today()
		// 	}));
		// });

		grunt.task.run([
			'clean:dist',

			// Common folder tasks
			'copy:bootstrapFontsDist',
			'copy:fonts',

			// Main specific tasks
			'useminPrepare:main',
			'sprite:main',
			'sass:main',
			'html2js:main',
			'copy:mainDist',

			// Even though there are no task definitions for these items, they
			// still need to be run because a config is created for them via
			// the useminPrepare tasks.
			'concat',
			'cssmin',
			'htmlmin',
			'uglify',

			// Hash the filenames to avoid browser caching.
			'rev',

			// Replace all the filenames with their hashed versions.
			'usemin:main-html',
			'usemin:main-css',
			'usemin:main-js',
		]);
	});

	grunt.registerTask('default', [
		'jshint',
		'build'
	]);

	grunt.registerTask('warn', [
		'jshint'
	]);
};
