'use strict';

angular.module('mainApp').controller('OrderDetailsCtrl', [
	'$scope',
	'$routeParams',
	'$location',
	'ConfigFactory',
	'OrderListFactory',
	'StrategyListFactory',
	function(
		$scope,
		$routeParams,
		$location,
		ConfigFactory,
		OrderListFactory,
		StrategyListFactory
	) {
		var orderData = null;

		// Save since it will be used in the rest of the app.
		ConfigFactory.setOutputFolder($routeParams.runName);

		$scope.ticker = $routeParams.ticker;
		$scope.orderId = $routeParams.orderId;

		$scope.orderDate = '';

		// Load all the overall strategies.
		$scope.strategies = [];

		OrderListFactory.getOverallOrders().then(function(data) {
			orderData = data[$scope.orderId];
			$scope.strategies = orderData.strategies;
			$scope.orderDate = orderData.buyDate;
			
			// Sort so the strategy with the highest is at the top and that is the one that is shown first.
			$scope.strategies.sort(function(a, b) {
				return b.winPercent - a.winPercent;
			});

			// Display the indicators from the winning strategy.
			var winningStrategy = $scope.strategies[0];
			if (winningStrategy) {
				StrategyListFactory.getStrategy(winningStrategy.name, $scope.ticker).then(function(data) {
					// Create an object for the order to be in so we can convert it to events.
					var order = {};
					order[$scope.orderId] = orderData;
					$scope.chartEvents = OrderListFactory.convertOrdersToDataSeries(order);
					
					// Set the chart to position to these dates.
					var buyDate = new Date($scope.orderDate);
					var range = 30; // Days
					$scope.extremes = {
						min: new Date(),
						max: new Date()
					};
					$scope.extremes.min.setTime(buyDate.getTime() - (range * 24 * 60 * 60 * 1000));
					$scope.extremes.max.setTime(buyDate.getTime() + (range * 24 * 60 * 60 * 1000));
					
					// Add all the indicators to the chart.
					for (var i = 0; i < data.indicators.length; i++) {
						$scope.$broadcast('AddIndicator', { name: data.indicators[i] });
					}
				});
			}
		});


		/**
		 * Called when the user clicks on a row in the strategy table
		 * @param  {Object} strategy The strategy row that was clicked
		 */
		$scope.strategyClick = function(strategy) {
			$location.url(ConfigFactory.getOutputName() + '/strategy/' + strategy.name + '/' + orderData.ticker);
		};

	} // end controller
]);

