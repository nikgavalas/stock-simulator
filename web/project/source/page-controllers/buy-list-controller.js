'use strict';

angular.module('mainApp').controller('BuyListCtrl', [
	'$scope',
	'$routeParams',
	'$location',
	'$window',
	'ConfigFactory',
	'BuyListFactory',
	'DateFactory',
	function(
		$scope,
		$routeParams,
		$location,
		$window,
		ConfigFactory,
		BuyListFactory,
		DateFactory
	) {


		function getNewList() {
			var date = $scope.buyListDate;
			if (!angular.isString(date)) {
				date = DateFactory.convertDateToString(date);
			}

			BuyListFactory.getBuyList(date).then(function(data) {
				$scope.buyList = data;

				// If we want to diplay only the top 10.
				//$scope.buyList = $scope.buyList.slice(0, 4);
			});		
		}

		// Save since it will be used in the rest of the app.
		ConfigFactory.setOutputFolder($routeParams.runName);

		$scope.calendarDate = new Date();
		$scope.buyListDate = $routeParams.date;
		if (!$scope.buyListDate) {
			$scope.buyListDate = DateFactory.convertDateToString(new Date());
		}

		$scope.buyList = [];

		$scope.setDate = function() {
			// TODO: use moment.js for better date manipulation...
			$scope.calendarDate = new Date($scope.buyListDate);
			if (!DateFactory.isValidDate($scope.calendarDate)) {
				$scope.calendarDate = new Date();
			}
		};


		$scope.$watch('calendarDate', function(newValue) {
			$scope.buyListDate = DateFactory.convertDateToString($scope.calendarDate);
			getNewList();
		});

	} // end controller
]);

