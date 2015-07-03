'use strict';

angular.module('mainApp').controller('OrderDetailsCtrl', [
	'$scope',
	'$routeParams',
	'$location',
	'$window',
	'$timeout',
	'ConfigFactory',
	'OrderListFactory',
	'StrategyListFactory',
	function(
		$scope,
		$routeParams,
		$location,
		$window,
		$timeout,
		ConfigFactory,
		OrderListFactory,
		StrategyListFactory
	) {
		var orderData = null;

		// Save since it will be used in the rest of the app.
		ConfigFactory.setOutputFolder($routeParams.runName);
		ConfigFactory.setDataType($routeParams.type);

		$scope.ticker = $routeParams.ticker;
		$scope.orderId = $routeParams.orderId;

		$scope.orderDate = '';

		// Load all the overall strategies.
		$scope.strategies = [];

		OrderListFactory.getOverallOrders().then(function(data) {
			// Find the order
			var orderId = parseInt($scope.orderId, 10);
			for (var i = 0; i < data.length; i++) {
				if (orderId === data[i].id) {
					orderData = data[i];
					break;
				}
			}

			$scope.order = orderData;
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
					var range = 50; // Bars
					var rangeMs = range * ConfigFactory.getRangeInMilliseconds();
					$scope.extremes = {
						min: new Date(),
						max: new Date()
					};
					$scope.extremes.min.setTime(buyDate.getTime() - (rangeMs));
					$scope.extremes.max.setTime(buyDate.getTime() + (rangeMs));
					
					// Clear the indicators and add the indicators to show what they looked like
					// at the time this order was placed.
					$scope.$broadcast('ClearIndicators');
					for (var i = 0; i < orderData.dependentIndicators.length; i++) {
						$scope.$broadcast('AddIndicator', { name: orderData.dependentIndicators[i], orderId: orderData.id, chartName: 'lowerTimeframe' });
					}

					$timeout(function() {
						$scope.$broadcast('RedrawChart');
					}, 500);
				});
			}
		});


		/**
		 * Called when the user clicks on a row in the strategy table
		 * @param  {Object} strategy The strategy row that was clicked
		 */
		$scope.strategyClick = function(strategy, $event) {
			var url = ConfigFactory.getOutputName() + '/strategy/' + strategy.name + '/' + orderData.ticker + '/' + ConfigFactory.getSimDataType();

			if ($event && ($event.ctrlKey || $event.shiftKey)) {
				$window.open('#/' + url);
			}
			else {
				$location.url(url);
			}
		};

		/**
		 * Returns the order gain as a percent.
		 * @param   {Object} order Order to calculate from
		 * @returns {Number} see description
		 */
		$scope.getPercentGain = function(order) {
			if (order) {
				var percent = (((order.sellPrice - order.buyPrice) / order.buyPrice) * 100).toFixed(2);
				return order.orderType > 0 ? percent : -percent;
			}

			return 0;
		};

		/**
		 * Expose the absolute function to the scope html
		 * @param  {Number} value Value to return the absolute value for
		 * @return {Number}       Math.abs value
		 */
		$scope.abs = function(value) {
			return Math.abs(value);
		};

	} // end controller
]);

