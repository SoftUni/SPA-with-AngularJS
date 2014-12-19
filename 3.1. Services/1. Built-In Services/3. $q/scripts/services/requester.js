app.factory('requester', function requester($q, $http) {
	return function (method, url, headers, data) {
		var deferred = $q.defer();

		$http({
			method: method,
			url: url,
			headers: headers,
			data: data
		})
		.success(deferred.resolve)
		.error(deferred.reject);

		return deferred.promise;
	}
});