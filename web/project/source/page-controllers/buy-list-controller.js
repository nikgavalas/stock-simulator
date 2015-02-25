'use strict';

angular.module('mainApp').controller('BuyListCtrl', [
	'$scope',
	'$routeParams',
	'$location',
	'$window',
	'ConfigFactory',
	'BuyListFactory',
	function(
		$scope,
		$routeParams,
		$location,
		$window,
		ConfigFactory,
		BuyListFactory
	) {

		function convertDateToString(date) {
			var month = (date.getMonth() + 1).toString();
			if (month.length < 2) {
				month = '0' + month;
			}
			var day = (date.getDate()).toString();
			if (day.length < 2) {
				day = '0' + day;
			}
			return date.getFullYear() + '-' + month + '-' + day;
		}

		function isValidDate(d) {
			return Object.prototype.toString.call(d) === '[object Date]' && !isNaN(d.getTime());
		}

		function getNewList() {
			var date = $scope.buyListDate;
			if (!angular.isString(date)) {
				date = convertDateToString(date);
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
			$scope.buyListDate = convertDateToString(new Date());
		}

		$scope.buyList = [];

		$scope.setDate = function() {
			// TODO: use moment.js for better date manipulation...
			$scope.calendarDate = new Date($scope.buyListDate);
			if (!isValidDate($scope.calendarDate)) {
				$scope.calendarDate = new Date();
			}
		};


		$scope.$watch('calendarDate', function(newValue) {
			$scope.buyListDate = convertDateToString($scope.calendarDate);
			getNewList();
		});

	} // end controller
]);

