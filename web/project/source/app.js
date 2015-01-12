'use strict';

var mainApp = angular.module('mainApp', [
	'ngRoute',
	'ngAnimate',
	'ngTouch',
	'ngCookies',
	'ngSanitize',
	'ui.bootstrap',
	'templates-mainApp'
])
.config(['$routeProvider', '$httpProvider', '$locationProvider', function ($routeProvider, $httpProvider, $locationProvider) {
	
	// Setup CORS requests (cross domain ajax)
	$httpProvider.defaults.useXDomain = true;
	delete $httpProvider.defaults.headers.common['X-Requested-With'];

	// Only one route and page for now.
	$routeProvider
		.when('/:runName?', {
			templateUrl: 'views/main.html',
			controller: 'MainCtrl'
		})
		.otherwise({
			redirectTo: '/'
		});

	$locationProvider.html5Mode(false);
}]);

mainApp.factory('$exceptionHandler', [
	'$log',
	function($log) {
		return function (exception, cause) {
			var options = {
				alertException: false,
				logException: true
			};

			if (options.logException) {
				$log.error(exception.message + '\n' + exception.stack);
			}

			if (options.alertException) {
				alert(exception.message + '\n\n' + exception.stack);
			}
		};
	}
]);
