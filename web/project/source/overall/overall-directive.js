'use strict';

mainApp.directive('overallTable', [
	'$http',
	'ConfigFactory', 
	function(
		$http,
		ConfigFactory
	) {
		return {
			restrict: 'E',
			replace: true,
			templateUrl: 'source/overall/overall-directive.html',
			scope: {
			},
			link: function($scope) {

				// Get the data for the overall stats.
				$http.get(ConfigFactory.getOutputFolder() + 'overall.json').success(function(data) {
					$scope.fileData = data;
				});
			}
		};
	}
]);
