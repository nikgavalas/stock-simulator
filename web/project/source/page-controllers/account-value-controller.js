'use strict';

angular.module('mainApp').controller('AccountValueCtrl', [
	'$scope',
	'$routeParams',
	'$http',
	'ConfigFactory',
	'AccountSummaryFactory',
	function(
		$scope,
		$routeParams,
		$http,
		ConfigFactory,
		AccountSummaryFactory
	) {

		// Save since it will be used in the rest of the app.
		ConfigFactory.setOutputFolder($routeParams.runName);

		$scope.yearSummaries = {};

		$http.get(ConfigFactory.getOutputFolder() + 'overall-account.json').success(function(data) {
			$scope.chartData = data.accountValue;

			var currentYear = new Date($scope.chartData[0][0]).getUTCFullYear();
			var currentSummary = AccountSummaryFactory.newYearSummary(currentYear);
			$scope.yearSummaries[currentYear] = currentSummary;

			for (var i = 0; i < $scope.chartData.length; i++) {
				var timeAndValue = $scope.chartData[i];
				var dateOfItem = new Date(timeAndValue[0]);

				// Move to the next year if we need to.
				if (dateOfItem.getUTCFullYear() !== currentYear) {
					currentYear = dateOfItem.getUTCFullYear();
					currentSummary = AccountSummaryFactory.newYearSummary(currentYear);
					$scope.yearSummaries[currentYear] = currentSummary;
				}

				// Save the value for calculations.
				currentSummary.addValue(dateOfItem, timeAndValue[1]);
			}
		});

	} // end controller
]);

