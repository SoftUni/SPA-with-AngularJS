app.controller('AdDetailsController', 
	function ($scope, $routeParams, $location, adsData) {
		$scope.ad = adsData.getById($routeParams.adId);

        // http://localhost:1111/#/ads/77#asdasd
		console.log('absUrl: ', $location.absUrl());
		console.log('Protocol: ', $location.protocol());
		console.log('port: ', $location.port());
		console.log('host: ', $location.host());
		console.log('path: ', $location.path());
		console.log('search: ', $location.search());
		console.log('hash: ', $location.hash());
		console.log('url: ', $location.url());

		$scope.addNewAd = function () {
			$location.url('#/newAd');
		}
	}
);