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
				/**
				 * Expose the absolute function to the scope html
				 * @param  {Number} value Value to return the absolute value for
				 * @return {Number}       Math.abs value
				 */
				$scope.abs = function(value) {
					return Math.abs(value);
				};

				// Get the data for the overall stats.
				$http.get(ConfigFactory.getOutputFolder() + 'overall.json').success(function(data) {
					$scope.fileData = data;
				});
			}
		};
	}
]);
