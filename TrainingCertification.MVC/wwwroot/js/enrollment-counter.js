(function () {
  const seatBadges = document.querySelectorAll("[data-session-id]");
  if (!seatBadges.length) return;

  const notice = document.getElementById("live-update-notice");
  const showNotice = () => notice?.classList.remove("d-none");
  const hideNotice = () => notice?.classList.add("d-none");

  if (!window.signalR) {
    showNotice();
    return;
  }

  const hubUrl = document.body.dataset.enrollmentHubUrl || "/hubs/enrollment";
  const token = document.querySelector("meta[name='signalr-token']")?.content || "";
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl, token ? { accessTokenFactory: () => token } : undefined)
    .withAutomaticReconnect()
    .build();
  const sessionIds = [...new Set([...seatBadges]
    .map(x => Number(x.dataset.sessionId))
    .filter(Number.isFinite))];

  const joinSessionGroups = () => Promise.all(
    sessionIds.map(sessionId => connection.invoke("JoinSessionGroup", sessionId))
  );

  connection.on("UpdateEnrollmentCount", (sessionId, remainingSpots) => {
    document.querySelectorAll(`[data-session-id='${sessionId}']`).forEach(x => {
      x.textContent = `${remainingSpots} spots remaining`;
      x.classList.toggle("bg-danger", remainingSpots <= 0);
      x.classList.toggle("bg-success", remainingSpots > 0);
      x.classList.remove("bg-info", "text-dark");
      x.classList.add("text-white");
    });
    document.querySelectorAll(`[data-enroll-session-id='${sessionId}']`).forEach(button => {
      if (remainingSpots <= 0) {
        button.disabled = true;
        button.textContent = "Full";
      }
    });
  });

  connection.onreconnecting(showNotice);
  connection.onreconnected(() => {
    joinSessionGroups()
      .then(hideNotice)
      .catch(showNotice);
  });
  connection.onclose(showNotice);

  connection.start()
    .then(() => {
      return joinSessionGroups();
    })
    .then(hideNotice)
    .catch(showNotice);
})();
