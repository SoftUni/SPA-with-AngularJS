app.controller('AllAdsController', function($scope, adsData, $log) {
	adsData.getAll()
		.$promise
		.then(function (data) {
			$scope.data = data;
		}, function (error) {
			$log.error(error);
		})
});