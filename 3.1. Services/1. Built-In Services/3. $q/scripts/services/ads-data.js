app.factory('adsData', function adsData($http, $q, requester) {
	function getAllAds(success, error) {
		// You can move out requester logic with promises in other service and use it:
		// return requester('GET', 'http://softuni-ads.azurewebsites.net/api/ads');

		var deferred = $q.defer();

		$http({
			method: 'GET',
			url: 'http://softuni-ads.azurewebsites.net/api/ads'
			// headers: {}
			// data: {}
		})
		.success(function (data, status, headers, config) {
			deferred.resolve(data, status, headers(), config);
		})
		.error(function (data, status, headers, config) {
			deferred.reject(data, status, headers(), config);
		});

		return deferred.promise;
	}

	return {
		getAds: getAllAds,
	}
})