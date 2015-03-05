'use strict';

angular.module('mainApp').controller('AccountValueCtrl', [
	'$scope',
	'$routeParams',
	'$http',
	'ConfigFactory',
	function(
		$scope,
		$routeParams,
		$http,
		ConfigFactory
	) {

		// Save since it will be used in the rest of the app.
		ConfigFactory.setOutputFolder($routeParams.runName);

		$http.get(ConfigFactory.getOutputFolder() + 'overall-account.json').success(function(data) {
			$scope.chartData = data.accountValue;
		});

	} // end controller
]);

