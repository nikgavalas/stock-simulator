'use strict';

angular.module('mainApp').controller('HigherTimeframeCtrl', [
	'$scope',
	'$routeParams',
	'HigherTimeframeFactory',
	'ConfigFactory',
	'DateFactory',
	function(
		$scope,
		$routeParams,
		HigherTimeframeFactory,
		ConfigFactory,
		DateFactory
	) {

		$scope.datesAndStates = [];

		function updateChartsAndTable() {
			var date = $scope.highFrameDate;
			if (!angular.isString(date)) {
				date = DateFactory.convertDateToString(date);
			}

			$scope.datesAndStates = [];

			HigherTimeframeFactory.get(date).then(function(data) {
				$scope.datesAndStates = data.statesData;
				$scope.$broadcast('CreateChartExistingData', { data: data.priceData, chartName: 'higherTimeframe' });
				$scope.$broadcast('AddIndicatorExistingData', { data: data.indicatorData, chartName: 'higherTimeframe' });
			});
		}

		// Save since it will be used in the rest of the app.
		ConfigFactory.setOutputFolder($routeParams.runName);

		$scope.calendarDate = new Date();
		$scope.highFrameDate = $routeParams.date;
		if (!$scope.highFrameDate) {
			$scope.highFrameDate = DateFactory.convertDateToString(new Date());
		}

		$scope.setDate = function() {
			// TODO: use moment.js for better date manipulation...
			$scope.calendarDate = new Date($scope.highFrameDate);
			if (!DateFactory.isValidDate($scope.calendarDate)) {
				$scope.calendarDate = new Date();
			}
		};


		$scope.$watch('calendarDate', function(newValue) {
			$scope.highFrameDate = DateFactory.convertDateToString($scope.calendarDate);
			updateChartsAndTable();
		});
	

	} // end controller
]);

