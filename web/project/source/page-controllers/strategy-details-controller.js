'use strict';

angular.module('mainApp').controller('StrategyDetailsCtrl', [
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
		// Save since it will be used in the rest of the app.
		ConfigFactory.setOutputFolder($routeParams.runName);
		ConfigFactory.setDataType($routeParams.type);

		$scope.strategy = $routeParams.strategy;
		$scope.ticker = $routeParams.ticker;
		$scope.activeOrder = null;

		$scope.orders = [];
		StrategyListFactory.getStrategy($scope.strategy, $scope.ticker).then(function(data) {
			$scope.orders = data.orders;
			$scope.strategyData = data;

			$scope.chartEvents = OrderListFactory.convertOrdersToDataSeries(data.orders);
		});


		/**
		 * Snap the chart to the order location.
		 * @param  {Object} order Order that was clicked
		 */
		$scope.orderClick = function(order) {
			$scope.activeOrder = order;
			
			// Clear the indicators and add the indicators to show what they looked like
			// at the time this order was placed.
			$scope.$broadcast('ClearIndicators');
			for (var i = 0; i < order.dependentIndicators.length; i++) {
				$scope.$broadcast('AddIndicator', { name: order.dependentIndicators[i], orderId: order.id, chartName: 'lowerTimeframe' });
			}

			// Set the chart to position to these dates.
			var buyDate = new Date(order.buyDate);
			var range = 100; // Bars
			var rangeMs = range * ConfigFactory.getRangeInMilliseconds();
			$scope.extremes = {
				min: new Date(),
				max: new Date()
			};
			$scope.extremes.min.setTime(buyDate.getTime() - (rangeMs));
			$scope.extremes.max.setTime(buyDate.getTime() + (rangeMs));
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

