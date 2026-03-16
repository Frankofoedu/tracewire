import { useEffect, useRef } from "react";
import type { HitlNotification } from "../types";

const BASE = "/v1";

export function useHitlStream(
  traceId: string | undefined,
  onNotification: (n: HitlNotification) => void,
) {
  const callbackRef = useRef(onNotification);
  callbackRef.current = onNotification;

  useEffect(() => {
    if (!traceId) return;

    const apiKey = localStorage.getItem("Tracewire_api_key") ?? "";
    const url = `${BASE}/traces/${traceId}/stream`;
    const eventSource = new EventSource(
      `${url}${url.includes("?") ? "&" : "?"}apiKey=${encodeURIComponent(apiKey)}`,
    );

    eventSource.onmessage = (e) => {
      try {
        const notification: HitlNotification = JSON.parse(e.data);
        callbackRef.current(notification);
      } catch {
        // ignore malformed messages
      }
    };

    return () => eventSource.close();
  }, [traceId]);
}
