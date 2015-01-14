'use strict';


angular.module('mainApp').controller('MainCtrl', [
	'$scope',
	'$routeParams',
	'ConfigFactory',
	function(
		$scope,
		$routeParams,
		ConfigFactory
	) {
		// Save since it will be used in the rest of the app.
		ConfigFactory.setOutputFolder($routeParams.runName);

	} // end controller
]);

