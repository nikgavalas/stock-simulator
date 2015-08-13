'use strict';

angular.module('mainApp').controller('HigherTimeframeCtrl', [
	'$scope',
	'$routeParams',
	'$timeout',
	'HigherTimeframeFactory',
	'ConfigFactory',
	'DateFactory',
	function(
		$scope,
		$routeParams,
		$timeout,
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

			HigherTimeframeFactory.get(date, $scope.ticker).then(function(data) {
				$scope.$broadcast('ClearIndicators');

				$scope.datesAndStates = data.statesData;
				$scope.$broadcast('CreateChartExistingData', { data: data.priceData, chartName: 'higherTimeframe' });
				$scope.$broadcast('AddIndicatorExistingData', { name: 'DtOsc', data: data.indicatorData[0], chartName: 'higherTimeframe' });
				$scope.$broadcast('AddIndicatorExistingData', { name: 'Atr', data: data.indicatorData[1], chartName: 'higherTimeframe' });
				$scope.$broadcast('AddIndicatorExistingData', { name: 'Keltner Channels', data: data.indicatorData[2], chartName: 'higherTimeframe' });
				$scope.$broadcast('AddIndicatorExistingData', { name: 'Sma', data: data.indicatorData[3], chartName: 'higherTimeframe' });

				$timeout(function() {
					$scope.$broadcast('RedrawChart');
				}, 500);
			});
		}

		// Save since it will be used in the rest of the app.
		ConfigFactory.setOutputFolder($routeParams.runName);

		$scope.ticker = $routeParams.ticker;

		$scope.highFrameDate = $routeParams.date;
		$scope.calendarDate = $scope.highFrameDate ? new Date(Date.parse($scope.highFrameDate)) : new Date();
		if (!$scope.highFrameDate) {
			$scope.highFrameDate = DateFactory.convertDateToString(new Date());
		}

		$scope.setDate = function() {
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

