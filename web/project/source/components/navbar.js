'use strict';

mainApp.directive('navbar', [
	'ConfigFactory',
	function(
		ConfigFactory
	) {
		return {
			restrict: 'E',
			replace: true,
			templateUrl: 'source/components/navbar.html',
			scope: {
				strategyName: '@'
			},
			link: function($scope) {
				$scope.buyListLink = '#/' + ConfigFactory.getOutputName() + '/buylist';
				$scope.accountValueLink = '#/' + ConfigFactory.getOutputName() + '/account';
				$scope.higherTimeframeLink = '#/' + ConfigFactory.getOutputName() + '/higher';
			}
		};
	}
]);
