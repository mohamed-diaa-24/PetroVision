/*
 * PetroVision real-time client (SignalR, server -> client only).
 *
 * Load the pinned SignalR browser client first, then this file:
 *   <script src="https://cdn.jsdelivr.net/npm/@microsoft/signalr@8.0.7/dist/browser/signalr.min.js"
 *           integrity="sha384-mU1xC5yC2LldSW74Rj1Ax8wPiLw/28V5eh51uKJMlBbRVsOtUYd4xyzNsgIAJARB"
 *           crossorigin="anonymous"></script>
 *   <script src="js/realtime.js"></script>
 *
 * Usage:
 *   const connection = connectRealtime({
 *     baseUrl: "http://localhost:5187",
 *     token: jwtFromLogin,                       // string, or a function returning the current token
 *     handlers: {
 *       AlertCreated: alert => renderAlert(alert),   // same AlertDto as GET /api/Alerts/{id}
 *       IncidentUpdated: incident => ...,
 *     },
 *   });
 *   await connection.start();
 *
 * Events: AlertCreated, AlertUpdated (AlertDto) · IncidentCreated, IncidentUpdated (IncidentDto)
 *         CameraStatusChanged ({ cameraId, health, lastSeenAt }) · CamerasChanged ({ action, cameraId, camera })
 */
(function (root) {
  "use strict";

  var EVENT_NAMES = [
    "AlertCreated",
    "AlertUpdated",
    "IncidentCreated",
    "IncidentUpdated",
    "CameraStatusChanged",
    "CamerasChanged"
  ];

  function connectRealtime(options) {
    options = options || {};
    var signalR = options.signalR || root.signalR;
    if (!signalR) {
      throw new Error("SignalR client is not loaded. Include @microsoft/signalr@8.0.7 before realtime.js.");
    }
    if (!options.baseUrl) {
      throw new Error("connectRealtime: baseUrl is required.");
    }

    var baseUrl = String(options.baseUrl).replace(/\/+$/, "");
    var token = options.token;
    var handlers = options.handlers || {};

    var connection = new signalR.HubConnectionBuilder()
      .withUrl(baseUrl + "/hubs/events", {
        accessTokenFactory: function () {
          return typeof token === "function" ? token() : token;
        },
        withCredentials: false
      })
      .withAutomaticReconnect()
      .configureLogging(options.logLevel != null ? options.logLevel : signalR.LogLevel.Warning)
      .build();

    EVENT_NAMES.forEach(function (name) {
      if (typeof handlers[name] === "function") {
        connection.on(name, handlers[name]);
      }
    });

    Object.keys(handlers).forEach(function (name) {
      if (EVENT_NAMES.indexOf(name) === -1) {
        console.warn("connectRealtime: unknown event '" + name + "' ignored. Known events: " + EVENT_NAMES.join(", "));
      }
    });

    return connection;
  }

  var api = { connectRealtime: connectRealtime, EVENT_NAMES: EVENT_NAMES.slice() };

  if (typeof module === "object" && module.exports) {
    module.exports = api;
  }
  root.connectRealtime = connectRealtime;
  root.PetroVisionRealtime = api;
})(typeof window !== "undefined" ? window : globalThis);
