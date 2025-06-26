// Copyright (c) Microsoft. All rights reserved.

import {
    Button,
    Field,
    Input,
    Label,
    MessageBar,
    MessageBarBody,
    MessageBarTitle,
    Spinner,
    Table,
    TableBody,
    TableCell,
    TableCellLayout,
    TableColumnDefinition,
    TableHeader,
    TableHeaderCell,
    TableRow,
    Textarea,
    createTableColumn,
    makeStyles,
    shorthands,
    tokens,
} from '@fluentui/react-components';
import { 
    AddRegular, 
    DeleteRegular, 
    NavigationRegular, 
    ArrowLeftRegular,
    ArrowRightRegular,
    ArrowClockwiseRegular,
    CameraRegular,
    PlayRegular,
    StopRegular,
    WindowRegular
} from '@fluentui/react-icons';
import React, { useState, useRef, useEffect } from 'react';
import { TabView } from './TabView';

const useClasses = makeStyles({
    container: {
        display: 'flex',
        flexDirection: 'column',
        ...shorthands.gap(tokens.spacingVerticalM),
        height: '100%',
    },
    section: {
        ...shorthands.padding(tokens.spacingVerticalM),
        ...shorthands.border('1px', 'solid', tokens.colorNeutralStroke2),
        borderRadius: tokens.borderRadiusMedium,
        backgroundColor: tokens.colorNeutralBackground1,
    },
    browserSection: {
        ...shorthands.padding(tokens.spacingVerticalM),
        ...shorthands.border('1px', 'solid', tokens.colorNeutralStroke2),
        borderRadius: tokens.borderRadiusMedium,
        backgroundColor: tokens.colorNeutralBackground1,
        flexGrow: 1,
        display: 'flex',
        flexDirection: 'column',
        minHeight: '600px',
    },
    browserControls: {
        display: 'flex',
        alignItems: 'center',
        ...shorthands.gap(tokens.spacingHorizontalS),
        ...shorthands.margin(0, 0, tokens.spacingVerticalM, 0),
        ...shorthands.padding(tokens.spacingVerticalS),
        backgroundColor: tokens.colorNeutralBackground2,
        borderRadius: tokens.borderRadiusMedium,
    },
    urlInput: {
        flexGrow: 1,
    },
    browserFrame: {
        width: '100%',
        height: '500px',
        border: '1px solid',
        borderColor: tokens.colorNeutralStroke2,
        borderRadius: tokens.borderRadiusMedium,
        backgroundColor: tokens.colorNeutralBackground3,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        flexDirection: 'column',
        ...shorthands.gap(tokens.spacingVerticalM),
    },
    actualFrame: {
        width: '100%',
        height: '100%',
        border: 'none',
        borderRadius: tokens.borderRadiusMedium,
    },
    configForm: {
        display: 'flex',
        flexDirection: 'column',
        ...shorthands.gap(tokens.spacingVerticalS),
    },
    actionButtons: {
        display: 'flex',
        ...shorthands.gap(tokens.spacingHorizontalS),
    },
    table: {
        backgroundColor: tokens.colorNeutralBackground1,
    },
    statusIndicator: {
        display: 'flex',
        alignItems: 'center',
        ...shorthands.gap(tokens.spacingHorizontalXS),
    },
    statusDot: {
        width: '8px',
        height: '8px',
        borderRadius: '50%',
    },
    active: {
        backgroundColor: tokens.colorPaletteGreenBackground3,
    },
    inactive: {
        backgroundColor: tokens.colorPaletteRedBackground3,
    },
    loading: {
        backgroundColor: tokens.colorPaletteYellowBackground3,
    },
});

interface BrowserSession {
    id: string;
    name: string;
    currentUrl: string;
    status: 'closed' | 'starting' | 'active' | 'loading' | 'error';
    createdAt: Date;
    lastActiveAt: Date;
}

interface BrowserAction {
    id: string;
    timestamp: Date;
    action: string;
    parameters: Record<string, any>;
    response?: any;
    success: boolean;
}

export const BrowserTab: React.FC = () => {
    const classes = useClasses();
    const iframeRef = useRef<HTMLIFrameElement>(null);
    
    // Browser state
    const [sessions, setSessions] = useState<BrowserSession[]>([
        {
            id: '1',
            name: 'Main Browser',
            currentUrl: 'about:blank',
            status: 'closed',
            createdAt: new Date(),
            lastActiveAt: new Date(),
        }
    ]);
    
    const [activeSessionId, setActiveSessionId] = useState<string>('1');
    const [currentUrl, setCurrentUrl] = useState<string>('https://www.google.com');
    const [isLoading, setIsLoading] = useState(false);
    const [actions, setActions] = useState<BrowserAction[]>([]);
    
    // MCP Script execution
    const [scriptContent, setScriptContent] = useState('');
    const [scriptResults, setScriptResults] = useState<string>('');

    // Form state for new session
    const [newSession, setNewSession] = useState({
        name: '',
        url: 'https://www.google.com',
    });

    const activeSession = sessions.find(s => s.id === activeSessionId);

    const { columns: sessionColumns, rows: sessionRows } = useSessionTable(
        sessions,
        (id: string) => {
            setActiveSessionId(id);
        },
        (id: string) => {
            void handleStartSession(id);
        },
        (id: string) => {
            handleCloseSession(id);
        },
        (id: string) => {
            handleDeleteSession(id);
        }
    );

    const { columns: actionColumns, rows: actionRows } = useActionTable(actions);

    // Browser actions
    async function handleNavigate() {
        if (!activeSession || !currentUrl) return;
        
        setIsLoading(true);
        try {
            // Update session status
            setSessions(prev => prev.map(s => 
                s.id === activeSessionId ? { 
                    ...s, 
                    status: 'loading' as const,
                    currentUrl: currentUrl,
                    lastActiveAt: new Date()
                } : s
            ));

            // Load URL in iframe
            if (iframeRef.current) {
                iframeRef.current.src = currentUrl;
            }

            // Log action
            const action: BrowserAction = {
                id: Date.now().toString(),
                timestamp: new Date(),
                action: 'navigate',
                parameters: { url: currentUrl },
                success: true,
            };
            setActions(prev => [action, ...prev]);

            // Simulate loading
            await new Promise(resolve => setTimeout(resolve, 2000));

            // Update to active
            setSessions(prev => prev.map(s => 
                s.id === activeSessionId ? { 
                    ...s, 
                    status: 'active' as const,
                    lastActiveAt: new Date()
                } : s
            ));

        } catch (error) {
            console.error('Failed to navigate:', error);
            setSessions(prev => prev.map(s => 
                s.id === activeSessionId ? { ...s, status: 'error' as const } : s
            ));

            const action: BrowserAction = {
                id: Date.now().toString(),
                timestamp: new Date(),
                action: 'navigate',
                parameters: { url: currentUrl },
                success: false,
                response: { error: error instanceof Error ? error.message : 'Unknown error' },
            };
            setActions(prev => [action, ...prev]);
        } finally {
            setIsLoading(false);
        }
    }

    async function handleStartSession(id: string) {
        setSessions(prev => prev.map(s => 
            s.id === id ? { 
                ...s, 
                status: 'starting' as const,
                lastActiveAt: new Date()
            } : s
        ));

        // Simulate session start
        await new Promise(resolve => setTimeout(resolve, 1000));

        setSessions(prev => prev.map(s => 
            s.id === id ? { 
                ...s, 
                status: 'active' as const,
                lastActiveAt: new Date()
            } : s
        ));

        const action: BrowserAction = {
            id: Date.now().toString(),
            timestamp: new Date(),
            action: 'start_session',
            parameters: { sessionId: id },
            success: true,
        };
        setActions(prev => [action, ...prev]);
    }

    function handleCloseSession(id: string) {
        setSessions(prev => prev.map(s => 
            s.id === id ? { ...s, status: 'closed' as const } : s
        ));

        if (id === activeSessionId && iframeRef.current) {
            iframeRef.current.src = 'about:blank';
        }

        const action: BrowserAction = {
            id: Date.now().toString(),
            timestamp: new Date(),
            action: 'close_session',
            parameters: { sessionId: id },
            success: true,
        };
        setActions(prev => [action, ...prev]);
    }

    function handleDeleteSession(id: string) {
        setSessions(prev => prev.filter(s => s.id !== id));
        if (id === activeSessionId) {
            const remainingSession = sessions.find(s => s.id !== id);
            if (remainingSession) {
                setActiveSessionId(remainingSession.id);
            }
        }
    }

    function handleAddSession() {
        if (!newSession.name) return;

        const session: BrowserSession = {
            id: Date.now().toString(),
            name: newSession.name,
            currentUrl: newSession.url,
            status: 'closed',
            createdAt: new Date(),
            lastActiveAt: new Date(),
        };

        setSessions(prev => [...prev, session]);
        setNewSession({ name: '', url: 'https://www.google.com' });
    }

    function handleTakeScreenshot() {
        if (!activeSession) return;

        const action: BrowserAction = {
            id: Date.now().toString(),
            timestamp: new Date(),
            action: 'screenshot',
            parameters: { sessionId: activeSessionId },
            success: true,
            response: { message: 'Screenshot captured (simulated)' },
        };
        setActions(prev => [action, ...prev]);
    }

    async function handleExecuteScript() {
        if (!activeSession || !scriptContent) return;

        setIsLoading(true);
        try {
            // Simulate script execution
            await new Promise(resolve => setTimeout(resolve, 1000));
            
            const result = `Script executed successfully.\nInput: ${scriptContent}\nOutput: Script execution simulated in demo mode.`;
            setScriptResults(result);

            const action: BrowserAction = {
                id: Date.now().toString(),
                timestamp: new Date(),
                action: 'execute_script',
                parameters: { script: scriptContent },
                success: true,
                response: { result },
            };
            setActions(prev => [action, ...prev]);

        } catch (error) {
            const errorMsg = error instanceof Error ? error.message : 'Unknown error';
            setScriptResults(`Error: ${errorMsg}`);

            const action: BrowserAction = {
                id: Date.now().toString(),
                timestamp: new Date(),
                action: 'execute_script',
                parameters: { script: scriptContent },
                success: false,
                response: { error: errorMsg },
            };
            setActions(prev => [action, ...prev]);
        } finally {
            setIsLoading(false);
        }
    }

    // Load active session URL when session changes
    useEffect(() => {
        if (activeSession && activeSession.status === 'active' && activeSession.currentUrl !== 'about:blank') {
            setCurrentUrl(activeSession.currentUrl);
            if (iframeRef.current && activeSession.currentUrl) {
                iframeRef.current.src = activeSession.currentUrl;
            }
        }
    }, [activeSession]);

    return (
        <TabView
            title="Browser"
            learnMoreDescription="Browser Integration with MCP"
            learnMoreLink="https://modelcontextprotocol.io/"
        >
            <div className={classes.container}>
                {/* Session Management */}
                <div className={classes.section}>
                    <Label size="large" weight="semibold">Browser Sessions</Label>
                    
                    {/* Add New Session Form */}
                    <div className={classes.configForm}>
                        <Field label="Session Name">
                            <Input
                                value={newSession.name}
                                onChange={(_, data) => {
                                    setNewSession(prev => ({ ...prev, name: data.value }));
                                }}
                                placeholder="e.g., Research Session"
                            />
                        </Field>
                        
                        <Field label="Initial URL">
                            <Input
                                value={newSession.url}
                                onChange={(_, data) => {
                                    setNewSession(prev => ({ ...prev, url: data.value }));
                                }}
                                placeholder="https://www.google.com"
                            />
                        </Field>
                        
                        <div className={classes.actionButtons}>
                            <Button
                                icon={<AddRegular />}
                                onClick={handleAddSession}
                                disabled={!newSession.name}
                            >
                                Add Session
                            </Button>
                        </div>
                    </div>

                    {/* Sessions Table */}
                    <Table aria-label="Browser Sessions" className={classes.table}>
                        <TableHeader>
                            <TableRow>
                                {sessionColumns.map((column) => column.renderHeaderCell())}
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {sessionRows.map((item) => (
                                <TableRow 
                                    key={item.id}
                                    style={{
                                        backgroundColor: item.id === activeSessionId ? 
                                            tokens.colorNeutralBackground1Pressed : 'transparent'
                                    }}
                                >
                                    {sessionColumns.map((column) => column.renderCell(item))}
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                </div>

                {/* Browser Window */}
                <div className={classes.browserSection}>
                    <Label size="large" weight="semibold">
                        Browser Window {activeSession ? `- ${activeSession.name}` : ''}
                    </Label>
                    
                    {activeSession ? (
                        <>
                            {/* Browser Controls */}
                            <div className={classes.browserControls}>
                                <Button
                                    icon={<ArrowLeftRegular />}
                                    size="small"
                                    disabled={activeSession.status !== 'active'}
                                    title="Back"
                                />
                                <Button
                                    icon={<ArrowRightRegular />}
                                    size="small"
                                    disabled={activeSession.status !== 'active'}
                                    title="Forward"
                                />
                                <Button
                                    icon={<ArrowClockwiseRegular />}
                                    size="small"
                                    disabled={activeSession.status !== 'active'}
                                    title="Refresh"
                                />
                                
                                <Input
                                    value={currentUrl}
                                    onChange={(_, data) => { setCurrentUrl(data.value); }}
                                    className={classes.urlInput}
                                    disabled={activeSession.status === 'loading'}
                                    placeholder="Enter URL..."
                                />
                                
                                <Button
                                    icon={<NavigationRegular />}
                                    onClick={() => { void handleNavigate(); }}
                                    disabled={isLoading || !currentUrl}
                                >
                                    {isLoading ? 'Loading...' : 'Go'}
                                </Button>
                                
                                <Button
                                    icon={<CameraRegular />}
                                    size="small"
                                    onClick={() => { handleTakeScreenshot(); }}
                                    disabled={activeSession.status !== 'active'}
                                    title="Screenshot"
                                />
                            </div>

                            {/* Browser Frame */}
                            <div className={classes.browserFrame}>
                                {activeSession.status === 'active' || activeSession.status === 'loading' ? (
                                    <>
                                        {activeSession.status === 'loading' && (
                                            <div style={{ position: 'absolute', zIndex: 10 }}>
                                                <Spinner size="large" />
                                            </div>
                                        )}
                                        <iframe
                                            ref={iframeRef}
                                            className={classes.actualFrame}
                                            src={activeSession.currentUrl !== 'about:blank' ? activeSession.currentUrl : undefined}
                                            title="Browser Content"
                                            sandbox="allow-same-origin allow-scripts allow-forms allow-popups allow-popups-to-escape-sandbox"
                                        />
                                    </>
                                ) : (
                                    <>
                                        <WindowRegular style={{ fontSize: '48px', color: tokens.colorNeutralForeground3 }} />
                                        <Label>Browser session is {activeSession.status}</Label>
                                        {activeSession.status === 'closed' && (
                                            <Button
                                                icon={<PlayRegular />}
                                                onClick={() => { void handleStartSession(activeSession.id); }}
                                            >
                                                Start Session
                                            </Button>
                                        )}
                                    </>
                                )}
                            </div>
                        </>
                    ) : (
                        <MessageBar intent="info">
                            <MessageBarBody>
                                <MessageBarTitle>No Active Session</MessageBarTitle>
                                Please select or create a browser session to begin.
                            </MessageBarBody>
                        </MessageBar>
                    )}
                </div>

                {/* MCP Script Execution */}
                <div className={classes.section}>
                    <Label size="large" weight="semibold">MCP Script Execution</Label>
                    <div className={classes.configForm}>
                        <Field label="JavaScript Code">
                            <Textarea
                                value={scriptContent}
                                onChange={(_, data) => { setScriptContent(data.value); }}
                                placeholder="document.querySelector('title').textContent"
                                rows={4}
                            />
                        </Field>
                        
                        <div className={classes.actionButtons}>
                            <Button
                                icon={<PlayRegular />}
                                onClick={() => { void handleExecuteScript(); }}
                                disabled={!scriptContent || !activeSession || activeSession.status !== 'active' || isLoading}
                            >
                                Execute Script
                            </Button>
                        </div>
                        
                        {scriptResults && (
                            <Field label="Results">
                                <Textarea
                                    value={scriptResults}
                                    readOnly
                                    rows={4}
                                />
                            </Field>
                        )}
                    </div>
                </div>

                {/* Action Log */}
                <div className={classes.section}>
                    <Label size="large" weight="semibold">Action Log</Label>
                    <Table aria-label="Browser Actions" className={classes.table} size="small">
                        <TableHeader>
                            <TableRow>
                                {actionColumns.map((column) => column.renderHeaderCell())}
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {actionRows.map((item) => (
                                <TableRow key={item.id}>
                                    {actionColumns.map((column) => column.renderCell(item))}
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                </div>
            </div>
        </TabView>
    );
};

function useSessionTable(
    sessions: BrowserSession[],
    onSelect: (id: string) => void,
    onStart: (id: string) => void,
    onStop: (id: string) => void,
    onDelete: (id: string) => void
): { columns: Array<TableColumnDefinition<BrowserSession>>; rows: BrowserSession[] } {
    const classes = useClasses();

    const columns: Array<TableColumnDefinition<BrowserSession>> = [
        createTableColumn<BrowserSession>({
            columnId: 'name',
            renderHeaderCell: () => <TableHeaderCell>Name</TableHeaderCell>,
            renderCell: (item) => (
                <TableCell>
                    <TableCellLayout>{item.name}</TableCellLayout>
                </TableCell>
            ),
        }),
        createTableColumn<BrowserSession>({
            columnId: 'url',
            renderHeaderCell: () => <TableHeaderCell>Current URL</TableHeaderCell>,
            renderCell: (item) => (
                <TableCell>
                    <TableCellLayout>{item.currentUrl}</TableCellLayout>
                </TableCell>
            ),
        }),
        createTableColumn<BrowserSession>({
            columnId: 'status',
            renderHeaderCell: () => <TableHeaderCell>Status</TableHeaderCell>,
            renderCell: (item) => (
                <TableCell>
                    <TableCellLayout>
                        <div className={classes.statusIndicator}>
                            <div 
                                className={`${classes.statusDot} ${
                                    item.status === 'active' ? classes.active :
                                    item.status === 'loading' || item.status === 'starting' ? classes.loading :
                                    classes.inactive
                                }`}
                            />
                            {(item.status === 'loading' || item.status === 'starting') ? (
                                <>
                                    <Spinner size="tiny" />
                                    <span>{item.status}...</span>
                                </>
                            ) : (
                                <span>{item.status}</span>
                            )}
                        </div>
                    </TableCellLayout>
                </TableCell>
            ),
        }),
        createTableColumn<BrowserSession>({
            columnId: 'actions',
            renderHeaderCell: () => <TableHeaderCell>Actions</TableHeaderCell>,
            renderCell: (item) => (
                <TableCell>
                    <TableCellLayout>
                        <div className={classes.actionButtons}>
                            <Button
                                size="small"
                                onClick={() => { onSelect(item.id); }}
                                appearance="subtle"
                            >
                                Select
                            </Button>
                            {item.status === 'active' || item.status === 'loading' ? (
                                <Button
                                    size="small"
                                    icon={<StopRegular />}
                                    onClick={() => { onStop(item.id); }}
                                >
                                    Stop
                                </Button>
                            ) : (
                                <Button
                                    size="small"
                                    icon={<PlayRegular />}
                                    onClick={() => { onStart(item.id); }}
                                    disabled={item.status === 'starting'}
                                >
                                    Start
                                </Button>
                            )}
                            <Button
                                size="small"
                                icon={<DeleteRegular />}
                                onClick={() => { onDelete(item.id); }}
                                appearance="subtle"
                            >
                                Delete
                            </Button>
                        </div>
                    </TableCellLayout>
                </TableCell>
            ),
        }),
    ];

    return { columns, rows: sessions };
}

function useActionTable(actions: BrowserAction[]): { columns: Array<TableColumnDefinition<BrowserAction>>; rows: BrowserAction[] } {
    const columns: Array<TableColumnDefinition<BrowserAction>> = [
        createTableColumn<BrowserAction>({
            columnId: 'timestamp',
            renderHeaderCell: () => <TableHeaderCell>Time</TableHeaderCell>,
            renderCell: (item) => (
                <TableCell>
                    <TableCellLayout>
                        {item.timestamp.toLocaleTimeString()}
                    </TableCellLayout>
                </TableCell>
            ),
        }),
        createTableColumn<BrowserAction>({
            columnId: 'action',
            renderHeaderCell: () => <TableHeaderCell>Action</TableHeaderCell>,
            renderCell: (item) => (
                <TableCell>
                    <TableCellLayout>{item.action}</TableCellLayout>
                </TableCell>
            ),
        }),
        createTableColumn<BrowserAction>({
            columnId: 'success',
            renderHeaderCell: () => <TableHeaderCell>Status</TableHeaderCell>,
            renderCell: (item) => (
                <TableCell>
                    <TableCellLayout>
                        <span style={{ color: item.success ? tokens.colorPaletteGreenForeground1 : tokens.colorPaletteRedForeground1 }}>
                            {item.success ? '✓' : '✗'}
                        </span>
                    </TableCellLayout>
                </TableCell>
            ),
        }),
        createTableColumn<BrowserAction>({
            columnId: 'parameters',
            renderHeaderCell: () => <TableHeaderCell>Details</TableHeaderCell>,
            renderCell: (item) => (
                <TableCell>
                    <TableCellLayout>
                        <pre style={{ fontSize: '0.8em', margin: 0, maxWidth: '300px', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                            {JSON.stringify(item.parameters, null, 1)}
                        </pre>
                    </TableCellLayout>
                </TableCell>
            ),
        }),
    ];

    return { columns, rows: actions };
}