import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';

import { PushService } from 'Source/Services/push.service';
import { SignalRService } from 'Source/Services/signalr.service';

@Component({
    selector: 'status',
    templateUrl: './status.component.html'
})
export class StatusComponent implements OnInit, OnDestroy {

    private subscriptionCountSubscription: Subscription = null;
    private subscriptionLiveCountSubscription: Subscription = null;

    public subscriptionCount: number = 0;
    public isOffline: boolean = !navigator.onLine;
    public statsSubscribed: boolean = false;

    constructor(private pushService: PushService, private signalRService: SignalRService) { }

    ngOnInit(): void {
        this.subscriptionCountSubscription = this.pushService.count.subscribe((value: number) => {
            this.subscriptionCount = value;
        });
    }

    ngOnDestroy(): void {
        this.subscriptionCountSubscription.unsubscribe();
        this.stopLiveCount();        
    }

    private stopLiveCount(): void {
        if (this.subscriptionLiveCountSubscription != null) {
            this.subscriptionLiveCountSubscription.unsubscribe();
            this.subscriptionLiveCountSubscription = null;
            this.signalRService.stopConnection();
        }
    }

    toogleStats(): void {
        this.stopLiveCount();
        
        if (this.statsSubscribed) {
            this.signalRService.renewConnection();
            this.subscriptionLiveCountSubscription = this.signalRService.liveCount.subscribe((value: number) => {
                if (value > -1) {
                    this.subscriptionCount = value;
                }
            });
        }
        else {
            this.stopLiveCount();
        }
    }
}
