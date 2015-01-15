'use strict';

mainApp.directive('navbar', [
	function() {
		return {
			restrict: 'E',
			replace: true,
			templateUrl: 'source/components/navbar.html',
			scope: {
				strategyName: '@'
			},
			controller: [
				'$scope',
				'ConfigFactory',
				function(
					$scope,
					ConfigFactory
				) {
	

				}
			]
		};
	}
]);
